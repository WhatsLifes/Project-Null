using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("Whether the flashlight has been picked up")]
    public bool isPickedUp = false;

    [Header("Flashlight Settings")]
    public Light flashlight;  // Assign in Inspector
    public KeyCode toggleKey = KeyCode.F;

    [Header("Mannequin Freeze Settings")]
    [Tooltip("Maximum distance the flashlight can freeze mannequins")]
    public float freezeDistance = 20f;
    [Tooltip("Layer mask for mannequins (set to 'Enemy' layer)")]
    public LayerMask mannequinLayer;

    [Header("Battery Settings")]
    [Range(0f, 100f)]
    public float maxBattery = 100f;
    [Range(0f, 100f)]
    public float currentBattery = 100f;
    [Tooltip("Battery drain per second when flashlight is on")]
    public float drainRate = 10f;
    [Tooltip("Battery recharge per second when flashlight is off")]
    public float rechargeRate = 5f;
    [Tooltip("Minimum battery level required to turn on flashlight")]
    public float minimumBatteryToTurnOn = 5f;

    [Header("Battery Pickup Settings")]
    [Tooltip("Amount of battery restored per pickup (percentage)")]
    [Range(0f, 100f)]
    public float batteryPickupAmount = 25f;
    [Tooltip("Should battery pickup fill to max or add a fixed amount?")]
    public bool fillToMax = false;

    [Header("Flicker Settings")]
    [Tooltip("Battery level below which flickering starts")]
    public float flickerThreshold = 50f;
    [Tooltip("Minimum time between flickers (seconds)")]
    public float minFlickerInterval = 0.3f;
    [Tooltip("Maximum time between flickers (seconds)")]
    public float maxFlickerInterval = 2f;
    [Tooltip("How long each flicker lasts (seconds)")]
    public float flickerDuration = 0.08f;
    [Tooltip("Intensity reduction during flicker (0-1)")]
    [Range(0f, 1f)]
    public float flickerIntensityMultiplier = 0.3f;

    private bool isOn = false;
    private float nextFlickerTime = 0f;
    private float flickerEndTime = 0f;
    private float originalIntensity;
    private MannequinEnemy currentlyFrozenMannequin = null; // Track which mannequin is frozen

    void Start()
    {
        if (flashlight != null)
        {
            flashlight.enabled = false;  // start off
            originalIntensity = flashlight.intensity;  // Store original intensity
        }

        currentBattery = maxBattery;  // Start with full battery
    }

    void Update()
    {
        // Only allow flashlight usage if picked up
        if (!isPickedUp) return;

        HandleToggle();
        HandleBattery();
        HandleFlicker();
        HandleMannequinFreeze(); // NEW: Check for mannequins to freeze
    }

    void HandleToggle()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (!isOn && currentBattery >= minimumBatteryToTurnOn)
            {
                // Turn on
                isOn = true;
                flashlight.enabled = true;
            }
            else if (isOn)
            {
                // Turn off
                isOn = false;
                flashlight.enabled = false;

                // Unfreeze any frozen mannequin when turning off
                UnfreezeMannequin();
            }
            // If battery too low, do nothing (can't turn on)
        }
    }

    void HandleBattery()
    {
        if (isOn)
        {
            // Drain battery
            currentBattery -= drainRate * Time.deltaTime;
            currentBattery = Mathf.Max(0f, currentBattery);

            // Auto turn off if battery depleted
            if (currentBattery <= 0f)
            {
                isOn = false;
                flashlight.enabled = false;
                UnfreezeMannequin(); // Unfreeze when battery dies
            }
        }
        else
        {
            // Recharge battery
            currentBattery += rechargeRate * Time.deltaTime;
            currentBattery = Mathf.Min(maxBattery, currentBattery);
        }
    }

    void HandleFlicker()
    {
        if (!isOn || currentBattery > flickerThreshold)
        {
            // No flickering needed
            if (flashlight.intensity != originalIntensity)
                flashlight.intensity = originalIntensity;
            return;
        }

        // Currently in a flicker
        if (Time.time < flickerEndTime)
        {
            // Dim instead of turning completely off
            flashlight.intensity = originalIntensity * flickerIntensityMultiplier;

            // Unfreeze mannequin during flicker (they can move when light flickers!)
            if (currentlyFrozenMannequin != null)
            {
                currentlyFrozenMannequin.FlashlightUnfreeze();
                currentlyFrozenMannequin = null;
            }

            return;
        }

        // Restore intensity after flicker ends
        if (flashlight.intensity != originalIntensity)
            flashlight.intensity = originalIntensity;

        // Time to start a new flicker?
        if (Time.time >= nextFlickerTime)
        {
            // Inverse battery percent - MORE battery = more frequent flickers
            // This prevents crazy flickering at 0%
            float batteryPercent = currentBattery / flickerThreshold;

            // Use inverse for frequency - lower battery = LESS frequent (more stable intervals)
            float frequencyMultiplier = Mathf.Lerp(2f, 0.8f, batteryPercent);

            float randomInterval = Random.Range(
                minFlickerInterval * frequencyMultiplier,
                maxFlickerInterval * frequencyMultiplier
            );

            nextFlickerTime = Time.time + randomInterval;
            flickerEndTime = Time.time + flickerDuration;
        }
    }

    void HandleMannequinFreeze()
    {
        // Only freeze if flashlight is on
        if (!isOn)
        {
            UnfreezeMannequin();
            return;
        }

        // Raycast from flashlight to see if we're hitting a mannequin
        Ray ray = new Ray(flashlight.transform.position, flashlight.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, freezeDistance, mannequinLayer))
        {
            // Draw debug ray
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.cyan);

            // Check if we hit a mannequin
            MannequinEnemy mannequin = hit.collider.GetComponent<MannequinEnemy>();

            if (mannequin != null)
            {
                // If this is a new mannequin, unfreeze the old one first
                if (currentlyFrozenMannequin != null && currentlyFrozenMannequin != mannequin)
                {
                    currentlyFrozenMannequin.FlashlightUnfreeze();
                }

                // Freeze this mannequin
                mannequin.FlashlightFreeze();
                currentlyFrozenMannequin = mannequin;
            }
            else
            {
                // Hit something else, unfreeze
                UnfreezeMannequin();
            }
        }
        else
        {
            // Not hitting anything, unfreeze
            UnfreezeMannequin();
        }
    }

    void UnfreezeMannequin()
    {
        if (currentlyFrozenMannequin != null)
        {
            currentlyFrozenMannequin.FlashlightUnfreeze();
            currentlyFrozenMannequin = null;
        }
    }

    // Public method to be called by the pickup script
    public void Pickup()
    {
        isPickedUp = true;
    }

    /// <summary>
    /// Recharges the battery by a fixed amount or to full capacity
    /// Call this method when player picks up a battery
    /// </summary>
    /// <param name="amount">Optional: specific amount to recharge (overrides default)</param>
    /// <returns>True if battery was recharged, false if already full</returns>
    public bool RechargeBattery(float amount = -1f)
    {
        // Check if battery is already full
        if (currentBattery >= maxBattery)
        {
            Debug.Log("Battery is already full!");
            return false;
        }

        // Use custom amount if provided, otherwise use default
        float rechargeAmount = (amount > 0) ? amount : batteryPickupAmount;

        if (fillToMax)
        {
            // Fill to maximum capacity
            currentBattery = maxBattery;
            Debug.Log($"Battery fully recharged to {maxBattery}%");
        }
        else
        {
            // Add fixed amount
            float oldBattery = currentBattery;
            currentBattery += rechargeAmount;
            currentBattery = Mathf.Min(currentBattery, maxBattery); // Cap at max

            Debug.Log($"Battery recharged: {oldBattery:F1}% → {currentBattery:F1}%");
        }

        return true;
    }

    /// <summary>
    /// Get current battery percentage (0-100)
    /// </summary>
    public float GetBatteryPercentage()
    {
        return (currentBattery / maxBattery) * 100f;
    }

    /// <summary>
    /// Check if battery is below a certain threshold
    /// </summary>
    public bool IsBatteryLow(float threshold = 20f)
    {
        return currentBattery <= threshold;
    }

    /// <summary>
    /// Set battery to a specific value (useful for debugging or special events)
    /// </summary>
    public void SetBattery(float amount)
    {
        currentBattery = Mathf.Clamp(amount, 0f, maxBattery);
    }
}