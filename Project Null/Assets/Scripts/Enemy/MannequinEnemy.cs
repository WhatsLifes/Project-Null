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

    [Header("Combat Settings")]
    public float attackCooldown = 1.2f;
    public float attackDistanceBuffer = 1.5f;

    [Header("Flashlight Freeze Settings")]
    [Tooltip("How long the mannequin stays frozen after flashlight moves away (0 = instant unfreeze)")]
    public float freezeDuration = 2f;

    private enum EnemyState { Idle, Patrolling, Chasing, Attacking }
    private EnemyState currentState = EnemyState.Idle;

    private float lastAttackTime;
    private float defaultSpeed;
    private bool isAttacking = false;
    private bool isFrozenByFlashlight = false;
    private float freezeEndTime = 0f;
    private Animator animator;

    protected override void Awake()
    {
        base.Awake();

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
            Debug.LogWarning($"{gameObject.name}: Agent was not assigned, found and assigned automatically.");
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
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

        // Check if freeze duration has expired
        if (isFrozenByFlashlight && Time.time >= freezeEndTime)
        {
            // Unfreeze now
            isFrozenByFlashlight = false;

            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
            if (animator != null)
            {
                animator.speed = 1f; // Resume animation at normal speed
            }

        Debug.Log($"{gameObject.name} unfrozen - freeze duration expired");
        }

        // If still frozen, do nothing
        if (isFrozenByFlashlight)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        // If not hostile yet, patrol and check for player in vision cone
        if (!isActive)
        {
            Patrolling();

            // Use cone vision - player must be in cone AND within sight range
            if (distance <= sightRange && CanSeePlayer())
            {
                Debug.Log($"[{gameObject.name}] Player spotted in vision cone - ACTIVATING!");
                ActivateMannequin();
            }
            return;
        }

        // Hostile - use cone vision for tracking
        bool canSeePlayer = CanSeePlayer() && distance <= sightRange;

        if (canSeePlayer)
        {
            // Player visible in cone
            if (distance <= attackRange + attackDistanceBuffer)
            {
                // Attack
                SetState(EnemyState.Attacking);
                AttackPlayer();
            }
            else
            {
                // Chase
                SetState(EnemyState.Chasing);
                ChasePlayer();
            }
        }
        else
        {
            // Lost sight of player - return to patrol
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
            agent.stoppingDistance = 0f;
        }

        Debug.Log($"{gameObject.name} deactivated - back to passive patrol");
    }

    // Freezes the mannequin completely when flashlight is on it
    public void FlashlightFreeze()
    {
        if (isFrozenByFlashlight)
        {
            // Already frozen - just extend the freeze duration
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

        // NEW: Pause the animator
        if (animator != null)
        {
            animator.speed = 0f; // Freeze animation
        }

        Debug.Log($"{gameObject.name} frozen by flashlight for {freezeDuration} seconds");
    }

    // Called when flashlight moves away (mannequin stays frozen for duration)
    public void FlashlightUnfreeze()
    {
        // Don't actually unfreeze yet - just log
        // The Update() method will handle unfreezing after the duration
        if (isFrozenByFlashlight)
        {
            float remainingTime = freezeEndTime - Time.time;
            Debug.Log($"{gameObject.name} flashlight moved away - will unfreeze in {remainingTime:F1}s");
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        // Check if player is within the cone angle
        if (angle > attackConeAngle / 2f)
        {
            return false;
        }

        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

        CharacterController playerCC = player.GetComponent<CharacterController>();
        Vector3 targetPos;

        if (playerCC != null)
        {
            // Aim at the center - accounts for scale automatically
            float actualHeight = playerCC.height * player.lossyScale.y;
            targetPos = player.position + Vector3.up * (actualHeight / 2f);
        }
        else
        {
            targetPos = player.position + Vector3.up * 1.0f;
        }

        Vector3 directionToTarget = (targetPos - eyePos).normalized;

        // Use layerMask to ignore the Enemy layer
        int layerMask = ~LayerMask.GetMask("Enemy");

        // Raycast from eye position to player
        if (Physics.Raycast(eyePos, directionToTarget, out RaycastHit hit, sightRayDistance, layerMask))
        {
#if UNITY_EDITOR
            Debug.DrawRay(eyePos, directionToTarget * hit.distance,
                hit.collider.CompareTag("Player") ? Color.green : Color.red, 0.1f);
#endif

            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
            else
            {
                Debug.DrawRay(eyePos, directionToTarget * hit.distance, Color.yellow, 0.1f);
                return false;
            }
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
            if (isActive)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
        }
        // Handle collisions with other enemies
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Vector3 pushDirection = (transform.position - collision.transform.position).normalized;
            pushDirection.y = 0;

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

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Forward direction
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position + Vector3.up * eyeHeight,
                        transform.position + Vector3.up * eyeHeight + transform.forward * sightRayDistance);

        // Vision cone
        Gizmos.color = Color.yellow;
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;
        Vector3 right = Quaternion.Euler(0, attackConeAngle / 2f, 0) * transform.forward;
        Vector3 left = Quaternion.Euler(0, -attackConeAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(eyePos, eyePos + right * sightRayDistance);
        Gizmos.DrawLine(eyePos, eyePos + left * sightRayDistance);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange + attackDistanceBuffer);
    }
}