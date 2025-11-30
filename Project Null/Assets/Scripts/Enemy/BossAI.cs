using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public Transform crowPosition; // Position to move to when called by crow

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip normalAudio;
    public AudioClip chaseAudio;
    [Range(0f, 1f)] public float audioVolume = 1f;

    [Header("Detection Ranges")]
    [Tooltip("Radius where boss can hear player footsteps (largest)")]
    public float hearingRadius = 30f;
    [Tooltip("Radius where boss can see the player (medium)")]
    public float visionRadius = 15f;
    [Tooltip("Radius where boss performs instant kill (smallest)")]
    public float killRadius = 2f;

    [Header("Movement Speeds")]
    public float patrolSpeed = 2f;
    public float investigateSpeed = 3.5f;
    public float chaseSpeed = 5f;

    [Header("Attack Settings")]
    [Tooltip("Delay before executing instant kill attack")]
    public float attackChargeDelay = 0.5f;

    [Header("Patrol Settings")]
    public float patrolWaitTime = 3f;
    public float patrolRange = 20f;

    [Header("Debug")]
    public bool showDebugGizmos = true;

    // Private variables
    private enum BossState { Patrolling, Investigating, Chasing, Attacking, MovingToCrow }
    private BossState currentState = BossState.Patrolling;

    private Vector3 lastHeardPosition;
    private Vector3 patrolDestination;
    private bool isWaiting = false;
    private bool isAttacking = false;
    private float patrolWaitTimer = 0f;

    private void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        agent.speed = patrolSpeed;
        SetPatrolDestination();

        // Play normal ambient audio
        if (audioSource != null && normalAudio != null)
        {
            audioSource.clip = normalAudio;
            audioSource.loop = true;
            audioSource.volume = audioVolume;
            audioSource.Play();
        }
    }

    private void Update()
    {
        if (agent == null || player == null)
            return;

        if (isAttacking)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check for kill range (highest priority)
        if (distanceToPlayer <= killRadius && currentState == BossState.Chasing)
        {
            StartCoroutine(PerformInstantKill());
            return;
        }

        // Check for vision range (aggro)
        if (distanceToPlayer <= visionRadius)
        {
            SetState(BossState.Chasing);
            ChasePlayer();
            return;
        }

        // Check for hearing range (investigate)
        if (distanceToPlayer <= hearingRadius && IsPlayerMakingNoise())
        {
            // Update last heard position
            lastHeardPosition = player.position;

            if (currentState != BossState.Investigating && currentState != BossState.Chasing)
            {
                SetState(BossState.Investigating);
            }
        }

        // State machine
        switch (currentState)
        {
            case BossState.Patrolling:
                Patrol();
                break;

            case BossState.Investigating:
                Investigate();
                break;

            case BossState.Chasing:
                // Lost sight of player, go back to investigating
                if (distanceToPlayer > visionRadius)
                {
                    lastHeardPosition = player.position;
                    SetState(BossState.Investigating);
                }
                break;

            case BossState.MovingToCrow:
                // Check if reached crow position
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    SetState(BossState.Patrolling);
                }
                break;
        }
    }

    /// Call this function to make the boss move to the crow's position
    public void CallToCrowPosition()
    {
        if (crowPosition == null)
        {
            Debug.LogWarning("Crow position not assigned!");
            return;
        }

        SetState(BossState.MovingToCrow);
        agent.speed = chaseSpeed;
        agent.SetDestination(crowPosition.position);
        Debug.Log("Boss called to crow position!");
    }

    private void Patrol()
    {
        agent.speed = patrolSpeed;

        if (isWaiting)
        {
            patrolWaitTimer += Time.deltaTime;
            if (patrolWaitTimer >= patrolWaitTime)
            {
                isWaiting = false;
                SetPatrolDestination();
            }
            return;
        }

        // Check if reached patrol destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isWaiting = true;
            patrolWaitTimer = 0f;
        }
    }

    private void SetPatrolDestination()
    {
        // Patrol on the same half of the map as the player
        Vector3 basePosition = player.position;

        // Generate random point around player's half
        Vector2 randomCircle = Random.insideUnitCircle * patrolRange;
        Vector3 randomPoint = new Vector3(
            basePosition.x + randomCircle.x,
            transform.position.y,
            basePosition.z + randomCircle.y
        );

        // Sample on NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, patrolRange, NavMesh.AllAreas))
        {
            patrolDestination = hit.position;
            agent.SetDestination(patrolDestination);
        }
    }

    private void Investigate()
    {
        agent.speed = investigateSpeed;

        // Move to last heard position
        agent.SetDestination(lastHeardPosition);

        // If reached investigation point, go back to patrolling
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 1f)
        {
            SetState(BossState.Patrolling);
        }
    }

    private void ChasePlayer()
    {
        agent.speed = chaseSpeed;

        // Sample player position on NavMesh
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

    private IEnumerator PerformInstantKill()
    {
        if (isAttacking)
            yield break;

        isAttacking = true;
        SetState(BossState.Attacking);

        agent.isStopped = true;

        // Face the player
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        Debug.Log("Boss charging attack...");

        // Charge delay
        yield return new WaitForSeconds(attackChargeDelay);

        // Check if player is still in range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= killRadius)
        {
            // Instant kill
            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                Debug.Log("Boss insta killed you.");
                playerScript.TakeDamage(9999); // Instant kill
            }
        }

        yield return new WaitForSeconds(0.5f);

        agent.isStopped = false;
        isAttacking = false;

        // Return to chasing
        if (Vector3.Distance(transform.position, player.position) <= visionRadius)
        {
            SetState(BossState.Chasing);
        }
        else
        {
            SetState(BossState.Investigating);
        }
    }

    private bool IsPlayerMakingNoise()
    {
        // This will be checked by the player script
        // For now, return true if player is moving
        // Later, player script should set a flag or call a method when making noise

        // Placeholder: assume player makes noise if they exist
        // The actual implementation will check if player is crouched
        return true;
    }

    private void SetState(BossState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        Debug.Log($"Boss state changed to: {newState}");

        // Change audio based on state
        if (audioSource != null)
        {
            if (newState == BossState.Chasing || newState == BossState.Attacking)
            {
                if (chaseAudio != null && audioSource.clip != chaseAudio)
                {
                    audioSource.clip = chaseAudio;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            else
            {
                if (normalAudio != null && audioSource.clip != normalAudio)
                {
                    audioSource.clip = normalAudio;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos)
            return;

        // Hearing radius (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);

        // Vision radius (orange)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        // Kill radius (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, killRadius);

        // Last heard position
        if (lastHeardPosition != Vector3.zero)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(lastHeardPosition, 1f);
            Gizmos.DrawLine(transform.position, lastHeardPosition);
        }

        // Patrol destination
        if (currentState == BossState.Patrolling && patrolDestination != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(patrolDestination, 0.5f);
        }
    }
}
