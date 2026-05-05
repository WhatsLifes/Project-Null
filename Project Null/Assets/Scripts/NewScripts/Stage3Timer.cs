using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StageTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Total time in seconds before player health reaches 0")]
    public float timeLimitSeconds = 300f; // Default: 5 minutes

    [Header("Health Drain Settings")]
    [Tooltip("Player health will drain to 0 over the entire timer duration")]
    public bool drainHealth = true;

    [Header("Sanity Drain Settings")]
    [Tooltip("Should sanity drain during the timer?")]
    public bool drainSanity = true;

    [Tooltip("Sanity drain multiplier relative to health (1.0 = same rate, 2.0 = twice as fast, 0.5 = half speed)")]
    public float sanityDrainMultiplier = 1.5f;

    [Header("References")]
    public Player player;
    public Sanity sanitySystem;

    [Header("UI (Optional)")]
    public TMP_Text timerText;
    public Image warningPanel; // Optional red overlay when time is low
    public float warningFlashSpeed = 2f;
    [Tooltip("Time remaining when warning starts (seconds)")]
    public float warningStartTime = 60f;

    private float timeRemaining;
    private bool timerActive = false;
    private float healthDrainRate; // Calculated health per second
    private float sanityDrainRate; // Calculated sanity per second
    private int initialHealth;
    private float initialSanity;
    private bool playerRegenWasEnabled; // Store player's original regen state

    // Accumulators for fractional damage
    private float healthAccumulator = 0f;
    private float sanityAccumulator = 0f;

    void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }

        // Find sanity if not assigned
        if (sanitySystem == null)
        {
            sanitySystem = FindObjectOfType<Sanity>();
        }

        // Initialize timer (don't start yet)
        timeRemaining = timeLimitSeconds;
        timerActive = false;

        // Hide warning panel initially
        if (warningPanel != null)
        {
            Color c = warningPanel.color;
            c.a = 0f;
            warningPanel.color = c;
        }
    }

    void Update()
    {
        if (!timerActive) return;

        // Countdown timer
        timeRemaining -= Time.deltaTime;

        // Update UI
        UpdateTimerUI();

        // Drain health over time
        if (drainHealth && player != null && player.Health > 0)
        {
            // Accumulate fractional damage over time
            healthAccumulator += healthDrainRate * Time.deltaTime;

            // Only deal damage when we've accumulated at least 1 HP
            if (healthAccumulator >= 1f)
            {
                int drainAmount = Mathf.FloorToInt(healthAccumulator);
                healthAccumulator -= drainAmount; // Keep the remainder

                player.TakeDamage(drainAmount);
            }
        }

        // Drain sanity over time
        if (drainSanity && sanitySystem != null && sanitySystem.currentSanity > 0)
        {
            sanityAccumulator += sanityDrainRate * Time.deltaTime;

            if (sanityAccumulator >= 0.1f) // Drain every 0.1 sanity
            {
                float drainAmount = sanityAccumulator;
                sanityAccumulator = 0f;

                sanitySystem.TakeSanityDamage(drainAmount, false); // false = no camera shake
            }
        }

        // Time's up - ensure player is dead
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            timerActive = false;
            KillPlayer();
        }

        // Warning flash effect
        if (warningPanel != null && timeRemaining <= warningStartTime)
        {
            float alpha = Mathf.PingPong(Time.time * warningFlashSpeed, 0.3f);
            Color c = warningPanel.color;
            c.a = alpha;
            warningPanel.color = c;
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

        // Color code based on time remaining
        if (timeRemaining <= 10f)
        {
            timerText.color = Color.red;
        }
        else if (timeRemaining <= warningStartTime)
        {
            timerText.color = Color.yellow;
        }
        else
        {
            timerText.color = Color.white;
        }

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void KillPlayer()
    {
        if (player != null && player.Health > 0)
        {
            Debug.Log("Time's up! Player eliminated.");
            player.TakeDamage(player.Health); // Kill player - triggers PlayerDeath()
        }
    }

    // ===== PUBLIC METHODS =====

    /// <summary>
    /// Start the timer - call this from other scripts (e.g., when door opens)
    /// </summary>
    public void StartTimer()
    {
        if (timerActive)
        {
            Debug.LogWarning("Timer already running!");
            return;
        }

        // Store initial values
        if (player != null)
        {
            initialHealth = player.Health;

            // Disable player health regeneration during timer
            playerRegenWasEnabled = player.enableRegen;
            player.SetRegenerationEnabled(false);
        }

        if (sanitySystem != null)
        {
            initialSanity = sanitySystem.currentSanity;

            // Stop any ongoing sanity restoration
            sanitySystem.StopRestoration();
        }

        // Calculate drain rates (drain to 0 over the entire timer duration)
        if (timeLimitSeconds > 0)
        {
            healthDrainRate = (float)initialHealth / timeLimitSeconds;
            sanityDrainRate = initialSanity / timeLimitSeconds * sanityDrainMultiplier;
        }

        // Reset accumulators
        healthAccumulator = 0f;
        sanityAccumulator = 0f;

        // Reset timer
        timeRemaining = timeLimitSeconds;
        timerActive = true;

        Debug.Log($"Timer started! Duration: {timeLimitSeconds}s, Health drain rate: {healthDrainRate:F2}/s, Sanity drain rate: {sanityDrainRate:F2}/s");
    }

    public void PauseTimer()
    {
        timerActive = false;
        Debug.Log("Timer paused");
    }

    public void ResumeTimer()
    {
        if (!timerActive)
        {
            timerActive = true;
            Debug.Log("Timer resumed");
        }
    }

    public void StopTimer()
    {
        timerActive = false;
        timeRemaining = 0f;

        // Restore player regeneration if it was enabled
        if (player != null && playerRegenWasEnabled)
        {
            player.SetRegenerationEnabled(true);
        }

        Debug.Log("Timer stopped");
    }

    public void AddTime(float seconds)
    {
        timeRemaining += seconds;

        // Recalculate drain rates with new time
        if (timeRemaining > 0)
        {
            if (player != null && player.Health > 0)
            {
                healthDrainRate = (float)player.Health / timeRemaining;
            }

            if (sanitySystem != null && sanitySystem.currentSanity > 0)
            {
                sanityDrainRate = sanitySystem.currentSanity / timeRemaining * sanityDrainMultiplier;
            }
        }

        Debug.Log($"Added {seconds} seconds. New time: {timeRemaining:F1}s");
    }

    public void ResetTimer()
    {
        timeRemaining = timeLimitSeconds;
        timerActive = false;

        // Reset accumulators
        healthAccumulator = 0f;
        sanityAccumulator = 0f;

        // Restore player regeneration if it was disabled
        if (player != null && playerRegenWasEnabled)
        {
            player.SetRegenerationEnabled(true);
        }

        if (warningPanel != null)
        {
            Color c = warningPanel.color;
            c.a = 0f;
            warningPanel.color = c;
        }

        Debug.Log("Timer reset");
    }

    public bool IsTimerActive()
    {
        return timerActive;
    }

    public float GetTimeRemaining()
    {
        return timeRemaining;
    }

    private void OnDestroy()
    {
        // Cleanup: restore player regeneration if timer is destroyed while active
        if (player != null && playerRegenWasEnabled)
        {
            player.SetRegenerationEnabled(true);
        }
    }
}