using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CreatureEnemy : Enemy
{
    private enum EnemyState
    {
        Patrol,
        Stalk,
        Chase
    }

    [Header("State")]
    [SerializeField] private EnemyState state;

    private float stateTimer;
    private bool canStalk = true;
    private float stalkCooldownTimer;
    private float playerLookTimer;

    [Header("Senses")]
    public LayerMask LineOfSightMask;

    [Header("Settings")]
    public float roamSpeed = 2f;
    public float stalkSpeed = 1f;
    public float chaseSpeed = 6f;
    public float stalkCooldownTime = 15f;
    public float stopChaseDistance = 2f;

    [Header("Crawl Audio (Patrol / Chase)")]
    [SerializeField] private AudioSource crawlAudio;
    [SerializeField] private AudioClip crawlClip;
    [SerializeField] private float crawlVolume = 0.5f;

    [Header("Stalk Audio (Decision Layer)")]
    [SerializeField] private AudioSource stalkAudio;
    [SerializeField] private AudioClip stalkIdleClip;
    [SerializeField] private AudioClip stalkTenseClip;
    [SerializeField] private AudioClip stalkBreakClip;

    private bool initialized;

    // ---------------- INIT ----------------

    protected override void Awake()
    {
        base.Awake();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        agent.enabled = true;

        // Crawl audio setup
        if (crawlAudio == null)
            crawlAudio = gameObject.AddComponent<AudioSource>();

        crawlAudio.clip = crawlClip;
        crawlAudio.loop = true;
        crawlAudio.spatialBlend = 1f;
        crawlAudio.volume = 0f;
        crawlAudio.Play();

        // Stalk audio setup
        if (stalkAudio == null)
            stalkAudio = gameObject.AddComponent<AudioSource>();

        stalkAudio.loop = true;
        stalkAudio.spatialBlend = 1f;
        stalkAudio.volume = 1f;
        stalkAudio.pitch = .4f;
    }

    private void Start()
    {
        StartCoroutine(InitSafe());
    }

    private IEnumerator InitSafe()
    {
        yield return null;

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                agent.Warp(hit.position);
            else
            {
                Debug.LogWarning("Creature could not be placed on NavMesh.");
                yield break;
            }
        }

        agent.isStopped = false;
        agent.speed = roamSpeed;

        isActive = true;
        initialized = true;
    }

    // ---------------- UPDATE ----------------

    private void Update()
    {
        if (!initialized || !isActive || player == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        UpdateCooldown();

        float dist = Vector3.Distance(transform.position, player.position);

        bool inSight = dist <= sightRange && HasLineOfSight();
        bool looking = IsPlayerLookingAtCreature(15f);

        switch (state)
        {
            case EnemyState.Patrol:
                if (inSight && canStalk)
                    SetState(EnemyState.Stalk);
                else
                {
                    agent.speed = roamSpeed;
                    Patrolling();
                }
                break;

            case EnemyState.Stalk:
                StalkLogic(inSight, looking);
                break;

            case EnemyState.Chase:
                ChaseLogic();
                break;
        }

        UpdateCrawlAudio(dist);
        UpdateStalkAudio(looking);
    }

    // ---------------- STATES ----------------

    private void StalkLogic(bool inSight, bool looking)
    {
        if (!inSight)
        {
            SetState(EnemyState.Patrol);
            return;
        }

        agent.isStopped = true;

        Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookPos);

        if (looking)
            playerLookTimer += Time.deltaTime;
        else
            playerLookTimer = 0f;

        stateTimer += Time.deltaTime;

        if (!looking && stateTimer > 2f)
        {
            SetState(EnemyState.Chase);
            return;
        }

        if (playerLookTimer > 5f && playerLookTimer < 10f)
        {
            SetState(EnemyState.Patrol);
            return;
        }

        if (playerLookTimer >= 10f)
        {
            SetState(EnemyState.Chase);
            return;
        }
    }

    private void ChaseLogic()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // If close enough, go back to patrol
        if (distanceToPlayer <= stopChaseDistance)
        {
            state = EnemyState.Patrol; // or whatever your patrol enum is
            return;
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    private void SetState(EnemyState newState)
    {
        state = newState;
        stateTimer = 0f;

        switch (newState)
        {
            case EnemyState.Patrol:
                agent.isStopped = false;
                canStalk = true;
                break;

            case EnemyState.Stalk:
                agent.ResetPath();
                agent.isStopped = true;
                break;

            case EnemyState.Chase:
                canStalk = false;
                stalkCooldownTimer = stalkCooldownTime;
                agent.isStopped = false;
                break;
        }
    }

    // ---------------- COOLDOWN ----------------

    private void UpdateCooldown()
    {
        if (!canStalk)
        {
            stalkCooldownTimer -= Time.deltaTime;

            if (stalkCooldownTimer <= 0f)
            {
                canStalk = true;
                SetState(EnemyState.Patrol);
            }
        }
    }

    // ---------------- AUDIO: CRAWL (PATROL + CHASE) ----------------

    private void UpdateCrawlAudio(float dist)
    {
        if (!isActive || !initialized)
            return;

        float distFactor = Mathf.Clamp01(1f - (dist / sightRange));

        float volume = 0f;
        float pitch = 1f;

        if (state == EnemyState.Patrol)
        {
            volume = .5f + distFactor * 0.3f;
            pitch = 0.6f;
        }
        else if (state == EnemyState.Chase)
        {
            volume = .7f + distFactor * 0.6f;
            pitch = .7f;
        }
        
        crawlAudio.volume = Mathf.Lerp(crawlAudio.volume, volume, Time.deltaTime * 5f);
        crawlAudio.pitch = Mathf.Lerp(crawlAudio.pitch, pitch, Time.deltaTime * 5f);
    }

    // ---------------- AUDIO: STALK (DECISION LAYER) ----------------

    private void UpdateStalkAudio(bool looking)
    {
        
        AudioClip targetClip = stalkIdleClip;
        
        if (state == EnemyState.Chase || !initialized)
        {
            targetClip = stalkBreakClip;
        }
        else if (state != EnemyState.Stalk )
        {
            if (stalkAudio.isPlaying)
                stalkAudio.Stop();
            return;
        }
        

        if (!stalkAudio.isPlaying)
            stalkAudio.Play();

        
        if (stalkAudio.clip != targetClip)
        {
            stalkAudio.clip = targetClip;
            stalkAudio.Play();
        }

    }

    // ---------------- SENSING ----------------

    public bool HasLineOfSight()
    {
        Vector3 origin = player.position;
        Vector3 target = transform.position;
        Vector3 dir = (target - origin).normalized;

        float dist = Vector3.Distance(origin, target);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, LineOfSightMask))
            return false;

        return true;
    }

    public bool IsPlayerLookingAtCreature(float maxAngle = 30f)
    {
        Vector3 toCreature = (transform.position - player.position).normalized;
        float dot = Vector3.Dot(player.forward, toCreature);

        return dot >= Mathf.Cos(maxAngle * Mathf.Deg2Rad);
    }
}