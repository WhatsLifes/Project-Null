using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("Whether the flashlight has been picked up")]
    public bool isPickedUp = false;

    [Header("Flashlight Settings")]
    public Light flashlight;  // Assign in Inspector
    public KeyCode toggleKey = KeyCode.F;

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

    // Public method to be called by the pickup script
    public void Pickup()
    {
        isPickedUp = true;
    }

    // Optional: Display battery level in Inspector during play mode
    void OnGUI()
    {
        if (!isPickedUp)
        {
            GUI.Label(new Rect(10, 10, 200, 20), "Find the flashlight...");
            return;
        }

        GUI.Label(new Rect(10, 10, 200, 20), $"Battery: {currentBattery:F1} / {maxBattery:F1}");
        if (currentBattery < flickerThreshold && isOn)
        {
            GUI.Label(new Rect(10, 30, 200, 20), "⚠ Low Battery - Flickering");
        }
    }
}