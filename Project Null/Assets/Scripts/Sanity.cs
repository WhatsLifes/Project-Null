using UnityEngine;

public class Sanity : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f;
    public float currentSanity = 0f;
    public float passiveDrainPerMinute = 5f;


    [Header("Effects")]
    public Camera playerCamera;
    public AudioSource ringingAudio;
    public AudioSource[] randomSanitySounds;
    public Light[] lightsToFlicker;

    [Header("Flashlight Reference")]
    public FlashlightToggle flashlightToggle;

    private float passiveDrainTimer = 0f;
    private float[] flickerTimers;
    private bool[] lightStates;
    private float randomSoundTimer = 0f;

    private void Start()
    {
        currentSanity = 0f;
        RestoreSanity(maxSanity);
        flickerTimers = new float[lightsToFlicker.Length];
        lightStates = new bool[lightsToFlicker.Length];

        if (flashlightToggle == null)
            flashlightToggle = FindObjectOfType<FlashlightToggle>();

        for(int i = 0; i < lightsToFlicker.Length; i++)
        {
            flickerTimers[i] = Random.Range(0.1f, 1f);
            lightStates[i] = true;
        }

    }

    private void Update()
    {
        passiveDrainTimer += Time.deltaTime;
        if(passiveDrainTimer >= 60f)
        {
            TakeSanityDamage(passiveDrainPerMinute);
            passiveDrainTimer = 0f;
        }

        ApplySanityEffects();
        HandleLightFlicker();
        HandleRandomSanitySounds();
    }

    public void TakeSanityDamage(float amount)
    {
        currentSanity -= amount;
        currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
    }

    public void RestoreSanity(float amount)
    {
        currentSanity += amount;
        currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
    }

    private void ApplySanityEffects()
    {
        float sanityPercent = currentSanity / maxSanity;

        if (playerCamera != null)
            playerCamera.fieldOfView = Mathf.Lerp(40f, 60f, sanityPercent);

        if (ringingAudio != null)
        {
            ringingAudio.volume = Mathf.Lerp(0f, 1f, 1f - sanityPercent);
            if (!ringingAudio.isPlaying)
                ringingAudio.Play();
        }
            

        if (sanityPercent <= 0.5f && Random.value < 0.01f)
            Debug.Log("You hear footsteps around you...");
    }

    private void HandleRandomSanitySounds()
    {
        float sanityPercent = currentSanity / maxSanity;
        if (sanityPercent > 0.5f || randomSanitySounds.Length == 0)
            return;

        randomSoundTimer -= Time.deltaTime;
        if (randomSoundTimer <= 0f)
        {
            int index = Random.Range(0, randomSanitySounds.Length);
            AudioSource chosen = randomSanitySounds[index];

            if (!chosen.isPlaying)
                chosen.Play();

            randomSoundTimer = Random.Range(5f, Mathf.Lerp(20f, 5f, 1f - sanityPercent));
        }
    }

    private void HandleLightFlicker()
    {
        float sanityPercent = currentSanity / maxSanity;

        if (sanityPercent > 0.5f)
        {
            foreach (Light light in lightsToFlicker)
            {
                if (light == null) continue;

                if (flashlightToggle != null && light == flashlightToggle.flashlight)
                    continue;

                light.enabled = true;
            }
            return;
        }

        float minInterval = Mathf.Lerp(1.5f, 0.05f, 1f - sanityPercent);
        float maxInterval = Mathf.Lerp(3f, 0.3f, 1f - sanityPercent);

        for(int i = 0; i < lightsToFlicker.Length; i++)
        {
            Light light = lightsToFlicker[i];
            if (light == null) continue;

            if (flashlightToggle != null && light == flashlightToggle.flashlight)
            {
                if (!flashlightToggle.flashlight.enabled)
                    continue;

                flickerTimers[i] -= Time.deltaTime;
                if (flickerTimers[i] <= 0f)
                {
                    light.intensity = Random.Range(0.1f, 1f);
                    flickerTimers[i] = Random.Range(minInterval, maxInterval);
                }
            }
        }
    }
}
