using UnityEngine;

public class CrowAlert : MonoBehaviour
{
    [Header("References")]
    public BossAI bossAI;
    public Transform player;

    [Header("Detection Settings")]
    public float detectionRadius = 5f;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Cooldown Settings")]
    public float alertCooldown = 10f;
    private float cooldownTimer = 0f;
    private bool hasAlerted = false;

    [Header("Visual Feedback")]
    public GameObject visualIndicator;
    public float indicatorDuration = 1f;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (bossAI == null)
            bossAI = FindFirstObjectByType<BossAI>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (visualIndicator != null)
            visualIndicator.SetActive(false);
        
        // crows start out silent
        if (audioSource != null) audioSource.mute = true;
    }

    void Update()
    {
        if (player == null || bossAI == null) return;

        // Update cooldown
        if (hasAlerted)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                hasAlerted = false;
            }
        }

        // Check if player is in detection range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius && !hasAlerted)
        {
            Debug.Log($"Crow detected player at distance {distanceToPlayer:F2}, alerting boss!");
            AlertBoss();
        }
    }

    private void AlertBoss()
    {
        // Alert the boss regardless of player crouch state
        Debug.Log($"Crow at {transform.position} sending alert to boss for player at {player.position}");
        bossAI.OnCrowAlert(player.position);

        hasAlerted = true;
        cooldownTimer = alertCooldown;

        // Play sound effect
        if (audioSource != null)
        {
            audioSource.mute = false;
        }

        // Show visual indicator
        if (visualIndicator != null)
        {
            StartCoroutine(ShowIndicator());
        }

        Debug.Log($"Crow at {transform.position} alerted boss to player at {player.position}");
    }

    private System.Collections.IEnumerator ShowIndicator()
    {
        visualIndicator.SetActive(true);
        yield return new WaitForSeconds(indicatorDuration);
        visualIndicator.SetActive(false);
    }

    // Visualization in editor - for testing
    private void OnDrawGizmos()
    {
        // Detection radius - RED if alerted, CYAN if ready
        Gizmos.color = hasAlerted ? new Color(1f, 0f, 0f, 0.4f) : new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Crow icon above
        Gizmos.color = hasAlerted ? Color.red : Color.cyan;
        Vector3 iconPos = transform.position + Vector3.up * 2f;
        Gizmos.DrawWireSphere(iconPos, 0.3f);

        if (Application.isPlaying && player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);

            // Line to player when in range
            if (dist <= detectionRadius)
            {
                Gizmos.color = hasAlerted ? Color.red : Color.yellow;
                Gizmos.DrawLine(transform.position, player.position);
            }

            // Cooldown indicator
            if (hasAlerted && cooldownTimer > 0f)
            {
                Gizmos.color = Color.red;
                float progress = cooldownTimer / alertCooldown;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f, Vector3.one * 0.4f * progress);
            }
        }

        // Line to boss if assigned
        if (Application.isPlaying && bossAI != null)
        {
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
            Gizmos.DrawLine(transform.position + Vector3.up, bossAI.transform.position + Vector3.up * 2f);
        }
    }
}
