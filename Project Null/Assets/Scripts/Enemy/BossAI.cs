using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip normalAudio;
    public AudioClip chaseAudio;

    [Header("Hearing Settings")]
    public float hearingRadius = 15f;
    public float hearingMoveSpeed = 3.5f;

    [Header("Vision Settings")]
    public float visionRadius = 8f;
    public float chaseSpeed = 6f;

    [Header("Kill Settings")]
    public float killRadius = 2f;
    public float killChargeDelay = 0.5f;

    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;
    public float patrolWaitTime = 3f;
    public Vector2 mapBoundsX = new Vector2(-50f, 50f);
    public Vector2 mapBoundsZ = new Vector2(-50f, 50f);

    [Header("Debug")]
    public bool showDebugLogs = true;

    // State tracking
    private enum BossState { Patrolling, Investigating, Chasing, Charging }
    private BossState currentState = BossState.Patrolling;

    private Vector3 lastHeardPosition;
    private Vector3 patrolTarget;
    private float patrolWaitTimer;
    private bool isChargingKill = false;
    private float killChargeTimer;
    private bool hasPlayedChaseAudio = false;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        agent.speed = patrolSpeed;
        SetNewPatrolTarget();
        PlayNormalAudio();

        if (showDebugLogs)
        {
            Debug.Log("=== BOSS AI INITIALIZED ===");
            Debug.Log($"Hearing Radius: {hearingRadius}, Vision Radius: {visionRadius}, Kill Radius: {killRadius}");
            Debug.Log($"Player found: {player != null}, PlayerSoundProducer: {(player != null ? player.GetComponent<PlayerSoundProducer>() != null : false)}");
            Debug.Log($"NavMeshAgent enabled: {agent.enabled}, isOnNavMesh: {agent.isOnNavMesh}");
            Debug.Log($"Boss position: {transform.position}");

            if (!agent.isOnNavMesh)
            {
                Debug.LogError("BOSS IS NOT ON NAVMESH! Please bake NavMesh or move boss to valid NavMesh area!");
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Priority 1: Kill radius (instant kill)
        if (distanceToPlayer <= killRadius)
        {
            if (!isChargingKill)
            {
                StartKillCharge();
            }
            else
            {
                killChargeTimer -= Time.deltaTime;
                if (killChargeTimer <= 0f)
                {
                    KillPlayer();
                }
            }
            return;
        }
        else
        {
            isChargingKill = false;
        }

        // Priority 2: Vision radius (active chase)
        if (distanceToPlayer <= visionRadius)
        {
            if (currentState != BossState.Chasing)
            {
                EnterChaseState();
            }
            ChasePlayer();
            return;
        }
        else if (currentState == BossState.Chasing)
        {
            // Lost sight of player
            ExitChaseState();
        }

        // Priority 3: Hearing radius (investigate sound)
        PlayerSoundProducer soundProducer = player.GetComponent<PlayerSoundProducer>();
        if (soundProducer != null && soundProducer.IsMakingSound() && distanceToPlayer <= hearingRadius)
        {
            // Only change state if not already investigating
            if (currentState != BossState.Investigating)
            {
                lastHeardPosition = player.position;
                if (showDebugLogs) Debug.Log($"Boss heard player at {player.position}, distance: {distanceToPlayer:F2}");
                EnterInvestigateState();
            }
            else
            {
                // Already investigating - update position periodically
                lastHeardPosition = player.position;
            }
        }

        // Handle current state
        switch (currentState)
        {
            case BossState.Investigating:
                InvestigateSound();
                break;
            case BossState.Patrolling:
                Patrol();
                break;
        }
    }

    private void Patrol()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0f)
            {
                SetNewPatrolTarget();
            }
        }
    }

    private void SetNewPatrolTarget()
    {
        // Patrol on the same half of the map as the player
        if (player != null)
        {
            float playerX = player.position.x;
            float midpointX = (mapBoundsX.x + mapBoundsX.y) / 2f;

            Vector2 xBounds;
            if (playerX > midpointX)
            {
                // Player is on right half
                xBounds = new Vector2(midpointX, mapBoundsX.y);
            }
            else
            {
                // Player is on left half
                xBounds = new Vector2(mapBoundsX.x, midpointX);
            }

            float randomX = Random.Range(xBounds.x, xBounds.y);
            float randomZ = Random.Range(mapBoundsZ.x, mapBoundsZ.y);
            patrolTarget = new Vector3(randomX, player.position.y, randomZ);
        }
        else
        {
            float randomX = Random.Range(mapBoundsX.x, mapBoundsX.y);
            float randomZ = Random.Range(mapBoundsZ.x, mapBoundsZ.y);
            patrolTarget = new Vector3(randomX, 0f, randomZ);
        }

        // Sample nearest valid position on NavMesh
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(patrolTarget, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            patrolTarget = hit.position; // Update to actual reachable position
        }
        else
        {
            if (showDebugLogs) Debug.LogWarning("Failed to find valid patrol position on NavMesh!");
        }

        patrolWaitTimer = patrolWaitTime;
    }

    private void InvestigateSound()
    {
        agent.speed = hearingMoveSpeed;
        agent.isStopped = false;

        // Sample nearest valid position on NavMesh
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(lastHeardPosition, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);

            if (showDebugLogs)
            {
                Debug.Log($"Boss moving to NavMesh position: {hit.position}, distance: {agent.remainingDistance:F2}, pathStatus: {agent.pathStatus}");
            }
        }
        else
        {
            if (showDebugLogs) Debug.LogWarning($"No valid NavMesh position near {lastHeardPosition}!");
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Reached the sound location, return to patrol
            if (showDebugLogs) Debug.Log("Boss reached investigation point, returning to patrol");
            currentState = BossState.Patrolling;
            SetNewPatrolTarget();
        }
    }
    private void ChasePlayer()
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    private void EnterInvestigateState()
    {
        currentState = BossState.Investigating;
        agent.isStopped = false;
        agent.speed = hearingMoveSpeed;

        if (showDebugLogs)
        {
            Debug.Log($"Boss investigating sound at {lastHeardPosition}, distance: {Vector3.Distance(transform.position, lastHeardPosition):F2}");
            Debug.Log($"Boss current position: {transform.position}");
            Debug.Log($"Boss isOnNavMesh: {agent.isOnNavMesh}, hasPath: {agent.hasPath}");
        }

        // Use NavMesh sampling to ensure valid destination
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(lastHeardPosition, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogError($"Cannot find valid NavMesh position near {lastHeardPosition}! Is NavMesh baked?");
        }
    }
    private void EnterChaseState()
    {
        currentState = BossState.Chasing;
        agent.isStopped = false;
        PlayChaseAudio();
        if (showDebugLogs) Debug.Log("Boss entering chase state!");
    }

    private void ExitChaseState()
    {
        currentState = BossState.Patrolling;
        agent.isStopped = false;
        SetNewPatrolTarget();
        PlayNormalAudio();
        if (showDebugLogs) Debug.Log("Boss lost sight of player, returning to patrol");
    }

    private void StartKillCharge()
    {
        isChargingKill = true;
        killChargeTimer = killChargeDelay;
        currentState = BossState.Charging;
        agent.isStopped = true;

        // Optional: Add visual/audio feedback for charging
        Debug.Log("Boss is charging kill attack!");
    }

    private void KillPlayer()
    {
        Debug.Log("Player killed by boss!");

        // Trigger death - you can expand this with your death system
        SimpleFPS fps = player.GetComponent<SimpleFPS>();
        if (fps != null)
        {
            fps.canMove = false;
            fps.canLook = false;
        }

        // Add your death handling here (reload scene, game over screen, etc.)
        // Example: SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        agent.isStopped = false;
        isChargingKill = false;
    }

    // Called by crow scripts
    public void OnCrowAlert(Vector3 playerPosition)
    {
        lastHeardPosition = playerPosition;
        if (showDebugLogs) Debug.Log($"Boss alerted by crow! Heading to position: {playerPosition}");
        EnterInvestigateState();
    }

    private void PlayNormalAudio()
    {
        if (audioSource != null && normalAudio != null && !hasPlayedChaseAudio)
        {
            audioSource.clip = normalAudio;
            audioSource.loop = true;
            audioSource.Play();
        }
        hasPlayedChaseAudio = false;
    }

    private void PlayChaseAudio()
    {
        if (audioSource != null && chaseAudio != null)
        {
            audioSource.clip = chaseAudio;
            audioSource.loop = true;
            audioSource.Play();
            hasPlayedChaseAudio = true;
        }
    }

    // Visualization in editor - ALWAYS VISIBLE for testing
    private void OnDrawGizmos()
    {
        // Kill radius (red)
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, killRadius);

        // Vision radius (yellow)
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        // Hearing radius (bright magenta/purple)
        Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, hearingRadius);

        if (Application.isPlaying)
        {
            // Current state indicator above boss
            Gizmos.color = GetStateColor();
            Vector3 statePos = transform.position + Vector3.up * 3f;
            Gizmos.DrawWireCube(statePos, Vector3.one * 0.5f);

            // Patrol target
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(patrolTarget, 1f);
            Gizmos.DrawLine(transform.position, patrolTarget);

            // Last heard position - BRIGHT CYAN CUBE
            if (currentState == BossState.Investigating)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(lastHeardPosition, Vector3.one * 1.5f);
                Gizmos.DrawSphere(lastHeardPosition, 0.3f);

                // Thick line to show where boss is heading
                Gizmos.color = new Color(0f, 1f, 1f, 0.8f);
                Gizmos.DrawLine(transform.position + Vector3.up, lastHeardPosition + Vector3.up);
            }

            // Line to player with distance label
            if (player != null)
            {
                float dist = Vector3.Distance(transform.position, player.position);
                Gizmos.color = dist <= killRadius ? Color.red :
                              dist <= visionRadius ? Color.yellow :
                              dist <= hearingRadius ? new Color(1f, 0f, 1f, 0.7f) : Color.gray;
                Gizmos.DrawLine(transform.position, player.position);

                // Draw player position marker
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(player.position, 0.5f);
            }

            // NavMesh path visualization
            if (agent != null && agent.hasPath)
            {
                Gizmos.color = Color.magenta;
                Vector3[] corners = agent.path.corners;
                for (int i = 0; i < corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(corners[i], corners[i + 1]);
                }
            }
        }
    }

    private Color GetStateColor()
    {
        switch (currentState)
        {
            case BossState.Patrolling: return Color.green;
            case BossState.Investigating: return Color.cyan;
            case BossState.Chasing: return Color.yellow;
            case BossState.Charging: return Color.red;
            default: return Color.white;
        }
    }
}
