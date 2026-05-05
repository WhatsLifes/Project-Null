using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MannequinEnemy : Enemy
{
    [Header("Vision Settings")]
    public float attackConeAngle = 90f;
    public float sightRayDistance = 12f;
    public float eyeHeight = 1.5f;
    public bool showDebugGizmos = true;

    [Header("Movement Settings")]
    [Tooltip("Movement speed while patrolling")]
    public float patrolSpeed = 2f;
    [Tooltip("Movement speed while chasing player")]
    public float chaseSpeed = 5f;

    [Header("Combat Settings")]
    public float attackCooldown = 1.2f;
    public float attackDistanceBuffer = 1.5f;

    [Header("Flashlight Freeze Settings")]
    [Tooltip("How long the mannequin stays frozen after flashlight moves away (0 = instant unfreeze)")]
    public float freezeDuration = 2f;

    [Header("Animation Settings")]
    public Animator animator;
    [Tooltip("Bool parameter to alternate between attack animations")]
    public string attackTypeParameter = "AttackType";

    private enum EnemyState { Idle, Patrolling, Chasing, Attacking }
    private EnemyState currentState = EnemyState.Idle;

    private float lastAttackTime;
    private float defaultSpeed;
    private bool isAttacking = false;
    private bool isFrozenByFlashlight = false;
    private float freezeEndTime = 0f;
    private bool useAttack1 = true;

    protected override void Awake()
    {
        base.Awake();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
            Debug.LogWarning($"{gameObject.name}: Agent was not assigned, found and assigned automatically.");
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (agent != null)
        {
            defaultSpeed = agent.speed;
            patrolSpeed = defaultSpeed;
        }

        SetState(EnemyState.Idle);
    }

    private void Start()
    {
        StartCoroutine(StartPatrolling());
    }

    private IEnumerator StartPatrolling()
    {
        yield return new WaitForSeconds(0.5f);

        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = patrolSpeed;
            agent.stoppingDistance = 0f;
            agent.updatePosition = true;
            agent.updateRotation = true;
        }

        SetState(EnemyState.Patrolling);
        Debug.Log($"{gameObject.name} started patrolling (not hostile yet)");
    }

    private void Update()
    {

        if (isFrozenByFlashlight && Time.time >= freezeEndTime)
        {
            isFrozenByFlashlight = false;

            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }

            if (animator != null)
            {
                animator.speed = 1f;
            }

            Debug.Log($"{gameObject.name} unfrozen - freeze duration expired");
        }

        if (isFrozenByFlashlight)
            return;

        if (agent == null || player == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (!isActive)
        {
            Patrolling();

            if (distance <= sightRange && CanSeePlayer())
            {
                Debug.Log($"[{gameObject.name}] Player spotted in vision cone - ACTIVATING!");
                ActivateMannequin();
            }
            return;
        }

        bool canSeePlayer = CanSeePlayer() && distance <= sightRange;

        if (canSeePlayer)
        {
            if (distance <= attackRange + attackDistanceBuffer)
            {
                SetState(EnemyState.Attacking);
                AttackPlayer();
            }
            else
            {
                SetState(EnemyState.Chasing);
                ChasePlayer();
            }
        }
        else
        {
            isAttacking = false;
            SetState(EnemyState.Patrolling);
            Patrolling();
        }
    }

    private void ActivateMannequin()
    {
        if (isActive) return;

        isActive = true;

        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = chaseSpeed;
            agent.stoppingDistance = attackDistanceBuffer;
            agent.updatePosition = true;
            agent.updateRotation = true;
        }

        Debug.Log($"{gameObject.name} activated - player spotted!");
        SetState(EnemyState.Chasing);
    }

    public new void Deactivate()
    {
        isActive = false;
        isAttacking = false;
        SetState(EnemyState.Patrolling);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = false;
            agent.speed = patrolSpeed;
            agent.stoppingDistance = 0f;
        }

        Debug.Log($"{gameObject.name} deactivated - back to passive patrol");
    }

    public void FlashlightFreeze()
    {
        if (isFrozenByFlashlight)
        {
            freezeEndTime = Time.time + freezeDuration;
            return;
        }

        isFrozenByFlashlight = true;
        freezeEndTime = Time.time + freezeDuration;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        if (animator != null)
        {
            animator.speed = 0f;
        }

        Debug.Log($"{gameObject.name} frozen by flashlight for {freezeDuration} seconds");
    }

    public void FlashlightUnfreeze()
    {
        if (isFrozenByFlashlight)
        {
            freezeEndTime = Time.time + freezeDuration;
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (angle > attackConeAngle / 2f)
            return false;

        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

        CharacterController playerCC = player.GetComponent<CharacterController>();
        Vector3 targetPos;

        if (playerCC != null)
        {
            float actualHeight = playerCC.height * player.lossyScale.y;
            targetPos = player.position + Vector3.up * (actualHeight / 2f);
        }
        else
        {
            targetPos = player.position + Vector3.up * 1.0f;
        }

        Vector3 directionToTarget = (targetPos - eyePos).normalized;
        int layerMask = ~LayerMask.GetMask("Enemy");

        if (Physics.Raycast(eyePos, directionToTarget, out RaycastHit hit, sightRayDistance, layerMask))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    private new void ChasePlayer()
    {
        if (!agent.isOnNavMesh || player == null)
            return;

        agent.isStopped = false;
        agent.updateRotation = true;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = attackDistanceBuffer;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange + attackDistanceBuffer)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(player.position, out hit, 2.0f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            else
                agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    public override void AttackPlayer()
    {
        if (!agent.isOnNavMesh || player == null || THEMC == null)
            return;

        if (!agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        agent.updateRotation = false;
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0;

        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange + attackDistanceBuffer && Time.time >= lastAttackTime + attackCooldown)
        {
            if (animator != null)
            {
                if (!string.IsNullOrEmpty(attackTypeParameter))
                    animator.SetBool(attackTypeParameter, useAttack1);

                animator.SetTrigger("isAttacking");
                useAttack1 = !useAttack1;
            }

            THEMC.TakeDamage(damage);

            if (Sanity.Instance != null)
                Sanity.Instance.TakeSanityDamage(damage, true);

            lastAttackTime = Time.time;
        }

        if (distance > attackRange + attackDistanceBuffer && CanSeePlayer())
        {
            SetState(EnemyState.Chasing);
            agent.isStopped = false;
        }
    }

    private void SetState(EnemyState newState)
    {
        currentState = newState;

        if (agent != null && agent.isOnNavMesh)
        {
            if (newState == EnemyState.Patrolling)
                agent.speed = patrolSpeed;
            else
                agent.speed = chaseSpeed;
        }
    }
}