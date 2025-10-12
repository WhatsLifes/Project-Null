using UnityEngine;

public class ChaseFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    public float minIntensityMultiplier = 0.3f;  // Darkest multiplier (30% of original)
    public float maxIntensityMultiplier = 1.5f;  // Brightest multiplier (150% of original)
    public float returnSpeed = 2f;               // How fast light returns to normal

    [Header("Sine Wave Settings")]
    public float sineSpeed = 5f;                 // How fast the sine wave oscillates

    [Header("Noise Settings")]
    public bool addNoise = true;                 // Enable/disable random noise
    public float noiseAmount = 0.1f;             // How much random variation (0.1 = 10%)
    public float noiseSpeed = 0.05f;             // How often noise changes (lower = more frequent)

    private Light lightComponent;
    private float originalIntensity;
    private float nextNoiseTime = 0f;
    private float currentNoise = 0f;

    void Start()
    {
        lightComponent = GetComponent<Light>();
        originalIntensity = lightComponent.intensity;
    }

    void Update()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        bool anyChasing = false;
        foreach (Enemy enemy in enemies)
        {
            if (enemy.isChasingPlayer)
            {
                anyChasing = true;
                break;
            }
        }

        if (anyChasing)
        {
            // Sine wave flicker
            float sine = Mathf.Sin(Time.time * sineSpeed);
            float normalizedSine = (sine + 1f) / 2f;

            // Calculate base intensity from sine wave
            float minIntensity = originalIntensity * minIntensityMultiplier;
            float maxIntensity = originalIntensity * maxIntensityMultiplier;
            float baseIntensity = Mathf.Lerp(minIntensity, maxIntensity, normalizedSine);

            // Add noise on top
            if (addNoise)
            {
                // Update noise at intervals
                if (Time.time >= nextNoiseTime)
                {
                    currentNoise = Random.Range(-noiseAmount, noiseAmount) * originalIntensity;
                    nextNoiseTime = Time.time + noiseSpeed;
                }

                // Apply noise to base intensity
                lightComponent.intensity = baseIntensity + currentNoise;
            }
            else
            {
                lightComponent.intensity = baseIntensity;
            }
        }
        else
        {
            // Smooth return to normal
            lightComponent.intensity = Mathf.Lerp(
                lightComponent.intensity,
                originalIntensity,
                Time.deltaTime * returnSpeed
            );
        }
    }
}