using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MannequinEnemy : Enemy
{
    [Header("Vision Settings")]
    public float attackConeAngle = 90f;
    public float sightRayDistance = 12f;
    public bool showDebugGizmos = true;

    [Header("Combat Settings")]
    public float attackCooldown = 1.2f;
    public float attackRangeBuffer = 0.5f; // allows attacks within small margin

    private enum EnemyState { Idle, Patrolling, Chasing, Attacking }
    private EnemyState currentState = EnemyState.Idle;

    private float lastAttackTime;
    private float defaultSpeed;
    private bool isAttacking = false;

    protected override void Awake()
    {
        base.Awake();

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
            Debug.LogWarning($"{gameObject.name}: Agent was not assigned, found and assigned automatically.");
        }

        StartCoroutine(ActivateAfterStart());
    }

    private IEnumerator ActivateAfterStart()
    {
        yield return new WaitForSeconds(0.5f);
        Activate();

        if (agent != null)
        {
            defaultSpeed = agent.speed; 
            agent.updatePosition = true;
            agent.updateRotation = true;
            Debug.Log($"{gameObject.name}: DefaultSpeed set to {defaultSpeed}, Agent enabled: {agent.enabled}, Agent on NavMesh: {agent.isOnNavMesh}");
        }

        SetState(EnemyState.Patrolling);
    }

    private new void Update()
    {
        if (agent == null || player == null || !agent.enabled)
            return;

        // Check if we're active (from base Enemy class)
        if (!isActive)
            return;

        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"{gameObject.name}: NOT ON NAVMESH!");
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        // Check if player is within detection range (regardless of vision cone)
        if (distance <= sightRange)
        {
            // Player is close - start chasing
            agent.isStopped = false;

            // Sample the player's position on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(player.position, out hit, 2.0f, NavMesh.AllAreas))
            {
                // Set destination to the nearest valid NavMesh point near the player
                agent.SetDestination(hit.position);
            }
            else
            {
                // Fallback: try player's exact position
                agent.SetDestination(player.position);
            }

            Debug.Log($"Chasing! Distance={distance:F2}, Velocity={agent.velocity.magnitude:F2}, HasPath={agent.hasPath}, PathStatus={agent.pathStatus}");

            SetState(EnemyState.Chasing);

            // Attack while chasing if in range and cooldown is over
            if (distance <= attackRange + attackRangeBuffer && Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
        }
        else
        {
            // Out of range -> patrol
            isAttacking = false;
            Patrolling();
            SetState(EnemyState.Patrolling);
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dir = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);

        if (angle > attackConeAngle / 2f) return false;

        Vector3 eyePos = transform.position + Vector3.up * (1.0f * transform.lossyScale.y);
        if (Physics.Raycast(eyePos, dir, out RaycastHit hit, sightRayDistance))
        {
#if UNITY_EDITOR
            Debug.DrawRay(eyePos, dir * sightRayDistance, hit.collider.CompareTag("Player") ? Color.green : Color.red);
#endif
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    public override void AttackPlayer()
    {
        if (!agent.isOnNavMesh || player == null || THEMC == null)
            return;

        // Face the player while moving + keep moving while attacking
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

        float distance = Vector3.Distance(transform.position, player.position);

        // Attack if within range - no angle check, just distance
        if (distance <= attackRange + attackRangeBuffer)
        {
            Debug.Log($"{gameObject.name} attacks the player for {damage} damage!");
            THEMC.TakeDamage(damage);
            lastAttackTime = Time.time;
        }
    }

    private void SetState(EnemyState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            Debug.Log($"[{gameObject.name}] switched to {newState} state.");
        }
    }

    // vision cone function
    /*private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Forward direction
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * sightRayDistance);

        // Vision cone
        Gizmos.color = Color.yellow;
        Vector3 right = Quaternion.Euler(0, attackConeAngle / 2f, 0) * transform.forward;
        Vector3 left = Quaternion.Euler(0, -attackConeAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + right * sightRayDistance);
        Gizmos.DrawLine(transform.position, transform.position + left * sightRayDistance);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange + attackRangeBuffer);
    } */
}
