using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CreatureEnemy : Enemy
{
    [Header("Roaming Settings")]
    public float roamSpeed = 2f;
    public float startDelay = 0.5f;

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
        sightRange = 0f;
        attackRange = 0f;

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
        agent.stoppingDistance = 0f;
        agent.isStopped = false;

        isActive = true;

        Debug.Log($"{gameObject.name} started roaming.");
    }

    private new void Update()
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

        // PURE roaming only
        Patrolling();
    }

    // Hard disable all combat logic
    protected override void ChasePlayer() { }
    public override void AttackPlayer() { }
    
}