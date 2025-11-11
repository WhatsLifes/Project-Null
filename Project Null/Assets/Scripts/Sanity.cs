using UnityEngine;

public class Sanity : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f;
    public float currentSanity = 0f;
    public float passiveDrainPerMinute = 5f;

    [Header("Effects")]
    public Camera playerCamera;
    public AudioClip ringingClip;
    public AudioClip[] randomSanityClips;
    public Light[] lightsToFlicker;

    private float passiveDrainTimer = 0f;
    private float[] flickerTimers;
    private bool[] lightStates;
    private float randomSoundTimer = 0f;
    private float ringingTimer = 0f;
    private AudioSource ringingAudio;
    private AudioSource sanityAudioSource;

    public event System.Action<float, float> OnSanityChanged; // (current, max)

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        flickerTimers = new float[lightsToFlicker.Length];
        lightStates = new bool[lightsToFlicker.Length];

        for(int i = 0; i < lightsToFlicker.Length; i++)
        {
            flickerTimers[i] = Random.Range(0.1f, 1f);
            lightStates[i] = true;
        }

        ringingAudio = gameObject.AddComponent<AudioSource>();
        ringingAudio.spatialBlend = 0f;
        ringingAudio.loop = false;
        ringingAudio.playOnAwake = false;
        ringingAudio.volume = 0.2f;

        if (ringingClip != null)
            ringingAudio.clip = ringingClip;

        sanityAudioSource = gameObject.AddComponent<AudioSource>();
        sanityAudioSource.playOnAwake = false;
        sanityAudioSource.spatialBlend = 0f;
        sanityAudioSource.loop = false;
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
        
        // Invoke the event
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    public void RestoreSanity(float amount)
    {
        currentSanity += amount;
        currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
        
        // Invoke the event
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    private void ApplySanityEffects()
    {
        float sanityPercent = currentSanity / maxSanity;

        if (playerCamera != null)
            playerCamera.fieldOfView = Mathf.Lerp(40f, 60f, sanityPercent);

        if (ringingAudio != null && ringingClip != null)
        {
            ringingTimer -= Time.deltaTime;
            if (sanityPercent < 0.5f && !ringingAudio.isPlaying && ringingTimer <= 0f)
            {
                float volume = Mathf.Lerp(0.1f, 0.8f, 1f - sanityPercent);
                ringingAudio.volume = volume;
                ringingAudio.panStereo = Random.Range(-0.5f, 0.5f);

                ringingAudio.Play();

                ringingTimer = Random.Range(
                    Mathf.Lerp(30f, 8f, 1f - sanityPercent),
                    Mathf.Lerp(60f, 15f, 1f - sanityPercent)
                );
            }
        }
    }

    private void HandleRandomSanitySounds()
    {
        float sanityPercent = currentSanity / maxSanity;
        if (sanityPercent > 0.5f || randomSanityClips == null || randomSanityClips.Length == 0)
            return;

        randomSoundTimer -= Time.deltaTime;
        if (randomSoundTimer <= 0f)
        {
            int index = Random.Range(0, randomSanityClips.Length);
            AudioClip chosenClip = randomSanityClips[index];

            if (chosenClip == null || sanityAudioSource == null)
                return;

            if (!sanityAudioSource.isPlaying)
            {
                sanityAudioSource.clip = chosenClip;
                sanityAudioSource.volume = Mathf.Lerp(0.2f, 0.6f, 1f - sanityPercent);
                sanityAudioSource.panStereo = Random.Range(-1f, 1f);
                sanityAudioSource.Play();
            }

            randomSoundTimer = Random.Range(5f, Mathf.Lerp(20f, 5f, 1f - sanityPercent));
        }
    }

    private void HandleLightFlicker()
    {
        float sanityPercent = currentSanity / maxSanity;

        if(sanityPercent > 0.5f)
        {
            foreach(Light light in lightsToFlicker)
            {
                if (light == null) continue;
                light.intensity = 1f;
            }
            return;
        }
        float minInterval = Mathf.Lerp(1.5f, 0.05f, 1f - sanityPercent);
        float maxInterval = Mathf.Lerp(3f, 0.3f, 1f - sanityPercent);

        for(int i = 0; i < lightsToFlicker.Length; i++)
        {
            Light light = lightsToFlicker[i];
            if (light == null) continue;

            flickerTimers[i] -= Time.deltaTime;
            if (flickerTimers[i] <= 0f)
            {
                light.intensity = Random.Range(0.01f, 1f);
                flickerTimers[i] = Random.Range(minInterval, maxInterval);
            }
        }
    }
}