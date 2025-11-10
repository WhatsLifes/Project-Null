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
    public float attackDistanceBuffer = 1.5f; // Distance to maintain from player when attacking

    private enum EnemyState { Idle, Patrolling, Chasing, Attacking }
    private EnemyState currentState = EnemyState.Idle;

    private float lastAttackTime;
    private float defaultSpeed;
    private bool isAttacking = false;
    private bool isFrozenByFlashlight = false;

    protected override void Awake()
    {
        base.Awake();

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
            Debug.LogWarning($"{gameObject.name}: Agent was not assigned, found and assigned automatically.");
        }

        if (agent != null)
        {
            defaultSpeed = agent.speed;
        }

        SetState(EnemyState.Idle);
    }

    private void Start()
    {
        // Start patrolling (not hostile yet)
        StartCoroutine(StartPatrolling());
    }

    private IEnumerator StartPatrolling()
    {
        yield return new WaitForSeconds(0.5f);

        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = defaultSpeed;
            agent.stoppingDistance = 0f;
            agent.updatePosition = true;
            agent.updateRotation = true;
        }

        SetState(EnemyState.Patrolling);
        Debug.Log($"{gameObject.name} started patrolling (not hostile yet)");
    }

    private new void Update()
    {
        if (agent == null || player == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        // If frozen by flashlight, do nothing
        if (isFrozenByFlashlight)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        // If not hostile yet, just patrol and check for player
        if (!isActive)
        {
            Patrolling();

            // Check if player enters vision cone
            if (CanSeePlayer())
            {
                ActivateMannequin();
            }
            return;
        }

        // Check if player is within sight using cone detection
        if (CanSeePlayer() && distance <= sightRange)
        {
            // Player spotted in cone - chase them
            if (distance <= attackRange + attackDistanceBuffer)
            {
                // Stop and attack
                SetState(EnemyState.Attacking);
                AttackPlayer();
            }
            else
            {
                // Chase player
                SetState(EnemyState.Chasing);
                ChasePlayer();
            }
        }
        else
        {
            // Player not in sight - patrol
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
            agent.speed = defaultSpeed;
            agent.stoppingDistance = attackDistanceBuffer;
            agent.updatePosition = true;
            agent.updateRotation = true;
        }

        Debug.Log($"{gameObject.name} activated - player spotted!");
        SetState(EnemyState.Chasing);
    }

    public new void Deactivate()
    {
        // Make enemy non-hostile but keep patrolling
        isActive = false;
        isAttacking = false;
        SetState(EnemyState.Patrolling);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = false;
            agent.stoppingDistance = 0f; // Reset to patrol stopping distance
        }

        Debug.Log($"{gameObject.name} deactivated - back to passive patrol");
    }

    // Freezes the mannequin completely when flashlight is on it. Stops all movement and behavior.
    public void FlashlightFreeze()
    {
        if (isFrozenByFlashlight) return;

        isFrozenByFlashlight = true;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        Debug.Log($"{gameObject.name} frozen by flashlight");
    }

    // Unfreezes the mannequin when flashlight is turned off. Resumes previous behavior.
    public void FlashlightUnfreeze()
    {
        if (!isFrozenByFlashlight) return;

        isFrozenByFlashlight = false;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

        Debug.Log($"{gameObject.name} unfrozen - resuming behavior");
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        // Check if player is within the 90-degree cone
        if (angle > attackConeAngle / 2f) return false;

        // Raycast to check line of sight
        Vector3 eyePos = transform.position + Vector3.up * (1.0f * transform.lossyScale.y);
        if (Physics.Raycast(eyePos, dirToPlayer, out RaycastHit hit, sightRayDistance))
        {
#if UNITY_EDITOR
            Debug.DrawRay(eyePos, dirToPlayer * sightRayDistance, hit.collider.CompareTag("Player") ? Color.green : Color.red);
#endif
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
        agent.stoppingDistance = attackDistanceBuffer;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Keep chasing if player is beyond attack range
        if (distanceToPlayer > attackRange + attackDistanceBuffer)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(player.position, out hit, 2.0f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                agent.SetDestination(player.position);
            }
        }
        else
        {
            // In attack range, stop and attack
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    public override void AttackPlayer()
    {
        if (!agent.isOnNavMesh || player == null || THEMC == null)
            return;

        // Stop movement completely during attack
        if (!agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        // Face the player
        agent.updateRotation = false;
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0;

        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

        float distance = Vector3.Distance(transform.position, player.position);

        // Attack if within range and cooldown is over
        if (distance <= attackRange + attackDistanceBuffer && Time.time >= lastAttackTime + attackCooldown)
        {
            Debug.Log($"{gameObject.name} attacks the player for {damage} damage!");
            THEMC.TakeDamage(damage);
            lastAttackTime = Time.time;
        }

        // If player moves out of attack range but still in sight, resume chasing
        if (distance > attackRange + attackDistanceBuffer && CanSeePlayer())
        {
            SetState(EnemyState.Chasing);
            agent.isStopped = false;
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

    private void OnCollisionEnter(Collision collision)
    {
        if (!agent.enabled || !agent.isOnNavMesh) return;

        // Stop when colliding with player
        if (collision.gameObject.CompareTag("Player"))
        {
            if (isActive) // Only stop if hostile
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
        }
        // Handle collisions with other enemies
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // Push away from other enemies slightly
            Vector3 pushDirection = (transform.position - collision.transform.position).normalized;
            pushDirection.y = 0; // Keep on horizontal plane

            if (pushDirection.sqrMagnitude > 0.01f)
            {
                transform.position += pushDirection * 0.1f;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!agent.enabled || !agent.isOnNavMesh) return;

        // Keep agent stopped while colliding with player (only if hostile)
        if (collision.gameObject.CompareTag("Player") && isActive)
        {
            agent.velocity = Vector3.zero;
        }
        // Maintain separation from other enemies
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Vector3 pushDirection = (transform.position - collision.transform.position).normalized;
            pushDirection.y = 0;

            if (pushDirection.sqrMagnitude > 0.01f)
            {
                transform.position += pushDirection * 0.05f * Time.deltaTime;
            }
        }
    }

    /*private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Forward direction
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * sightRayDistance);

        // Vision cone (90 degrees)
        Gizmos.color = Color.yellow;
        Vector3 right = Quaternion.Euler(0, attackConeAngle / 2f, 0) * transform.forward;
        Vector3 left = Quaternion.Euler(0, -attackConeAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + right * sightRayDistance);
        Gizmos.DrawLine(transform.position, transform.position + left * sightRayDistance);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange + attackDistanceBuffer);
    } */
}
