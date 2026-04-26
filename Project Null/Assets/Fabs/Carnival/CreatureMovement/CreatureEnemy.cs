using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public class CreatureEnemy : Enemy
{
    private enum EnemyState { Idle, Patrolling, Stalking, Chasing, Attacking }
    
    [Header("Current State")]
    [SerializeField] private EnemyState currentState = EnemyState.Patrolling;
    
    [Header("Patrolling Settings")] 
    public float roamSpeed = 2f;
    public float startDelay = 0.5f;

    [Header("Stalking Settings")]
    public float stalkSpeed = 1f;
    public float stalkDelay = 3f;
    
    [Header("Chasing Settings")]
    public float chaseSpeed = 30f;
    public float chaseDuration = 5f;
    
    
    
    private bool initialized = false;
    

    protected override void Awake()
    {
        base.Awake();
    
        // Ensure agent exists
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                Debug.LogError($"{gameObject.name}: Missing NavMeshAgent!");
                return;
            }
        }
    
        // Disable combat completely
    
        initialized = true;
    }

    private void Start()
    {
        if (!initialized) return;
        StartCoroutine(InitRoaming());
    }

    private IEnumerator InitRoaming()
    {
        yield return new WaitForSeconds(startDelay);

        if (agent == null) yield break;

        agent.enabled = true;

        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: Could not place on NavMesh.");
                yield break;
            }
        }

        agent.speed = roamSpeed;
        agent.stoppingDistance = .5f;
        agent.isStopped = false;

        isActive = true;

        Debug.Log($"{gameObject.name} started roaming from CreatureEnemy.cs");
    }

    private void Update()
    {
        if (!initialized || !isActive) return;
        if (agent == null || !agent.enabled) return;

        if (!agent.isOnNavMesh)
        {
            // Try recovery
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: Lost NavMesh, disabling.");
                enabled = false;
                return;
            }
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        playerInSightRange = distanceToPlayer <= sightRange;
        playerInAttackRange = distanceToPlayer <= attackRange;

        if (isActive)
        {
            if (playerInSightRange && stalkingRoutine == null)
            {
                currentState = EnemyState.Stalking;
                stalkingRoutine = StartCoroutine(Stalking());
            }

            if (currentState == EnemyState.Patrolling)
            {
                //if no other state, default to patrolling
                StartPatrolling();
            }
        }
        
        
    }
    private Coroutine stalkingRoutine;
    IEnumerator Stalking()
    {
        agent.speed = stalkSpeed;
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        //wait stalkDelay seconds before attacking.    
        yield return new WaitForSeconds(stalkDelay);
        
        //If player is still in radius attack otherwise return to patrolling.
        if (playerInSightRange)
        {
            currentState = EnemyState.Chasing;
            StartChase();

        }
        else
        {
            currentState = EnemyState.Patrolling;
        }
    }

    private void StartPatrolling()
    {
        agent.speed = roamSpeed;
        Patrolling();
    }

    private void StartChase()
    {
        agent.speed = chaseSpeed;
        ChasePlayer();
    }

}