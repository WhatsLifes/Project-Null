using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;


public class BossAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip normalAudio;
    public AudioClip chaseAudio;
    [Header("Chase Audio System (NEW)")]
    public float chaseAudioPlayDuration = 10f;
    public float chaseAudioFadeDuration = 2f;
    public float chaseAudioCooldown = 5f;

    private float chaseAudioCooldownTimer = 0f;
    private Coroutine chaseAudioRoutine;
    private bool chaseAudioPlaying = false;

    [Header("Hearing Settings")]
    public float hearingRadius = 15f;
    public float hearingMoveSpeed = 3.5f;
    [Tooltip("Cooldown in seconds between hearing pings while player is making noise")]
    public float hearingPingCooldown = 1.5f;

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

    [Header("Crow Settings")]
    [Tooltip("Speed the boss uses when responding to a crow alert")]
    public float crowInvestigateSpeed = 4f;

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
    private float hearingPingTimer = 0f;
    private bool investigatingFromCrow = false;

    // Animations
    private Animator animator;
    private bool isCrawling = false;
    public Transform modelRoot;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Agent baseline configuration to avoid unintended stops
        agent.speed = patrolSpeed;
        agent.autoBraking = false;           // keep moving instead of slowing near destinations
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.angularSpeed = Mathf.Max(agent.angularSpeed, 120f);
        agent.acceleration = Mathf.Max(agent.acceleration, 16f);
        SetNewPatrolTarget();
        // Only switch back if chase audio is NOT playing
        if (!chaseAudioPlaying)
        {
            PlayNormalAudio();
        }

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

        // tick down timers
        if (hearingPingTimer > 0f)
            hearingPingTimer -= Time.deltaTime;

        if (chaseAudioCooldownTimer > 0f)
            chaseAudioCooldownTimer -= Time.deltaTime;

        // Set Movement State
        bool isMoving = agent.velocity.sqrMagnitude > 0.1f;
        animator.SetBool("isMoving", isMoving);

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
            // If we were charging but player left the kill radius, abort charge
            if (isChargingKill)
            {
                isChargingKill = false;
                // Make sure agent can move again
                if (agent != null)
                {
                    agent.isStopped = false;
                    agent.ResetPath();
                }

                // If we were in Charging state, move back to chase if player is visible, otherwise patrol
                if (currentState == BossState.Charging)
                {
                    if (distanceToPlayer <= visionRadius)
                    {
                        if (showDebugLogs) Debug.Log("Charge aborted: player still in vision. Resuming chase.");
                        EnterChaseState();
                    }
                    else
                    {
                        currentState = BossState.Patrolling;
                        if (showDebugLogs) Debug.Log("Charge aborted: player out of vision. Returning to patrol.");
                        SetNewPatrolTarget();
                    }
                }
            }
        }

        // Priority 2: Vision radius (active chase)
        if (distanceToPlayer <= visionRadius)
        {
            if (currentState != BossState.Chasing)
            {
                EnterChaseState();

                // Enter Crawl Posture
                if (!isCrawling)
                {
                    animator.SetTrigger("Transition");
                    isCrawling = true;

                    // rotate 180 so crawl direction matches walking direction
                    if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
                    rotateCoroutine = StartCoroutine(RotateModelSmooth(180f, 0.3f));

                }
            }
            ChasePlayer();
            return;
        }
        else if (currentState == BossState.Chasing)
        {
            // Lost sight of player
            ExitChaseState();

            if (isCrawling)
            {
                animator.SetTrigger("Transition");
                isCrawling = false;

                // rotate back to standing orientation
                if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
                rotateCoroutine = StartCoroutine(RotateModelSmooth(0f, 0.3f));

            }
        }

        // Priority 3: Hearing radius (investigate sound)
        PlayerSoundProducer soundProducer = player.GetComponent<PlayerSoundProducer>();
        if (soundProducer != null && soundProducer.IsMakingSound() && distanceToPlayer <= hearingRadius)
        {
            // Respect hearing ping cooldown so boss goes to general area instead of tracking every frame
            if (hearingPingTimer <= 0f)
            {
                lastHeardPosition = player.position;
                hearingPingTimer = hearingPingCooldown;

                if (currentState != BossState.Investigating)
                {
                    if (showDebugLogs) Debug.Log($"Boss heard player at {player.position} (cooldown {hearingPingCooldown:F2}s), distance: {distanceToPlayer:F2}");
                    EnterInvestigateState();
                }
                else
                {
                    // Already investigating: refresh target at cooldown interval
                    if (showDebugLogs) Debug.Log($"Boss refreshing investigation target: {lastHeardPosition}");
                }
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

        // Safety: if something (animation/other script) stopped the agent while we should be chasing, force resume
        if (currentState == BossState.Chasing && agent != null && agent.isStopped)
        {
            agent.isStopped = false;
            if (showDebugLogs) Debug.Log("Agent was stopped during chase; forcing resume and re-issuing destination.");
            ChasePlayer();
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
        agent.speed = investigatingFromCrow ? crowInvestigateSpeed : hearingMoveSpeed;
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
            investigatingFromCrow = false;
            SetNewPatrolTarget();
        }
    }
    private void ChasePlayer()
    {
        // Continuously pursue player's current position
        agent.isStopped = false;
        agent.speed = chaseSpeed;

        // If path is marked complete but we're not at the player, rebuild it
        if (agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance > agent.stoppingDistance)
        {
            agent.ResetPath();
        }

        // Sample player position on NavMesh to avoid invalid targets
        NavMeshHit hit;
        if (NavMesh.SamplePosition(player.position, out hit, 3f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // Fallback: directly try player's position
            agent.SetDestination(player.position);
        }
    }

    private void EnterInvestigateState()
    {
        currentState = BossState.Investigating;
        agent.isStopped = false;
        agent.speed = investigatingFromCrow ? crowInvestigateSpeed : hearingMoveSpeed;

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
        agent.ResetPath();
        agent.speed = chaseSpeed;

        if (player != null)
            agent.SetDestination(player.position);

        // NEW SYSTEM
        PlayChaseAudio();
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

        // Deal lethal damage to trigger proper death system
        Player playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.TakeDamage(playerScript.Health);
        }

        agent.isStopped = false;
        isChargingKill = false;
    }

    // Animation Events Integration
    // Call this from the attack animation at the start of the lunge
    public void OnAttackStart()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        currentState = BossState.Charging;
        if (showDebugLogs) Debug.Log("OnAttackStart: agent stopped and path reset.");
    }

    // Call this from the attack animation at the end of the lunge
    public void OnAttackFinished()
    {
        isChargingKill = false;
        if (agent != null)
        {
            agent.isStopped = false;
        }

        float distanceToPlayer = player != null ? Vector3.Distance(transform.position, player.position) : Mathf.Infinity;
        if (player != null && distanceToPlayer <= visionRadius)
        {
            if (showDebugLogs) Debug.Log("OnAttackFinished: player in vision, resuming chase.");
            EnterChaseState();
        }
        else
        {
            if (showDebugLogs) Debug.Log("OnAttackFinished: player not in vision, returning to patrol.");
            currentState = BossState.Patrolling;
            SetNewPatrolTarget();
        }
    }

    // Called by crow scripts
    public void OnCrowAlert(Vector3 playerPosition)
    {
        lastHeardPosition = playerPosition;
        investigatingFromCrow = true;
        if (showDebugLogs) Debug.Log($"Boss alerted by crow! Heading to position: {playerPosition} at speed {crowInvestigateSpeed}");
        EnterInvestigateState();
    }

    private void PlayNormalAudio()
    {
        if (audioSource == null || normalAudio == null)
            return;

        if (chaseAudioPlaying)
            return; // IMPORTANT FIX

        if (audioSource.isPlaying && audioSource.clip == normalAudio)
            return;

        audioSource.clip = normalAudio;
        audioSource.loop = true;
        audioSource.volume = 1f;
        audioSource.Play();
    }

    private void PlayChaseAudio()
    {
        if (audioSource == null || chaseAudio == null)
            return;

        if (chaseAudioPlaying)
            return;

        if (chaseAudioCooldownTimer > 0f)
            return;

        // prevent duplicate coroutine
        if (chaseAudioRoutine != null)
            return;

        chaseAudioRoutine = StartCoroutine(ChaseAudioRoutine());
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

    private Coroutine rotateCoroutine;

    IEnumerator RotateModelSmooth(float targetYRot, float duration)
    {
        Quaternion startRot = modelRoot.localRotation;
        Quaternion endRot = Quaternion.Euler(0f, targetYRot, 0f);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / duration);
            modelRoot.localRotation = Quaternion.Slerp(startRot, endRot, lerp);
            yield return null;
        }

        modelRoot.localRotation = endRot;
    }
    private IEnumerator ChaseAudioRoutine()
    {
        chaseAudioPlaying = true;

        audioSource.clip = chaseAudio;
        audioSource.loop = false;
        audioSource.volume = 1f;
        audioSource.Play();

        float playTime = Mathf.Min(chaseAudioPlayDuration, chaseAudio.length);

        // wait while clip plays
        yield return new WaitForSeconds(playTime);

        // fade out
        float startVolume = audioSource.volume;
        float t = 0f;

        while (t < chaseAudioFadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / chaseAudioFadeDuration);
            yield return null;
        }

        audioSource.Stop();

        // fixes
        chaseAudioPlaying = false;
        chaseAudioRoutine = null; // THIS was missing (major bug)
        chaseAudioCooldownTimer = chaseAudioCooldown;

        // return to normal audio AFTER chase clip fully ends
        PlayNormalAudio();
    }
}
