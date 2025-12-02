using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class Sanity : MonoBehaviour
{
    public static Sanity Instance;

    [Header("Sanity Settings")]
    public float maxSanity = 100f;
    public float currentSanity = 100f;  // Start at max instead of 0
    public float passiveDrainPerMinute = 5f;

    [Header("Sanity Restoration")]
    [Tooltip("Rate at which sanity restores per second when RestoreSanity is called")]
    public float restoreRatePerSecond = 5f;

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
    private Coroutine restoreCoroutine;

    [Header("Post Processing")] //Visual Effects
    public Volume postProcessVolume;

    [Header("Camera Shake")]
    public float shakeAmount = 1f;
    public float shakeSpeed = 30f;

    private float shakeTime;
    private Quaternion originalLocalRot;

    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private MotionBlur motionBlur;

    public event System.Action<float, float> OnSanityChanged; // (current, max)

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        flickerTimers = new float[lightsToFlicker.Length];
        lightStates = new bool[lightsToFlicker.Length];

        for (int i = 0; i < lightsToFlicker.Length; i++)
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

        //Visual Effects for Sanity
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out chromaticAberration);
            postProcessVolume.profile.TryGet(out lensDistortion);
            postProcessVolume.profile.TryGet(out motionBlur);
        }

        originalLocalRot = playerCamera.transform.localRotation;
        FindPlayerCamera();
    }

    private void Update()
    {
        passiveDrainTimer += Time.deltaTime;
        if (passiveDrainTimer >= 60f)
        {
            TakeSanityDamage(passiveDrainPerMinute, false);
            passiveDrainTimer = 0f;
        }

        ApplySanityEffects();
        HandleLightFlicker();
        HandleRandomSanitySounds();
        FindPlayerCamera();
    }

    //ShakeCamera function with a duration and intensity value
    public void ShakeCamera(float duration, float intensity)
    {
        if (playerCamera == null)
            return;

        shakeTime = Mathf.Max(shakeTime, duration);
        shakeAmount = Mathf.Max(shakeAmount, intensity);
    }

    public void TakeSanityDamage(float amount, bool playShake = true)
    {
        // Stop any ongoing restoration when taking damage
        if (restoreCoroutine != null)
        {
            StopCoroutine(restoreCoroutine);
            restoreCoroutine = null;
        }

        //Temporary cut out to not lose sanity when being attacked
        //currentSanity -= amount;
        currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);

        if (playShake)
            ShakeCamera(0.25f, 0.75f);

        // Invoke the event
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    public void RestoreSanity(float amount)
    {
        RestoreSanity(amount, restoreRatePerSecond);
    }

    public void RestoreSanity(float amount, float ratePerSecond)
    {
        // Stop any previous restoration
        if (restoreCoroutine != null)
            StopCoroutine(restoreCoroutine);

        restoreCoroutine = StartCoroutine(RestoreSanityOverTime(amount, ratePerSecond));
    }

    public void RestoreSanityInstant(float amount)
    {
        currentSanity += amount;
        currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);

        // Invoke the event
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    private IEnumerator RestoreSanityOverTime(float totalAmount, float ratePerSecond)
    {
        float amountRestored = 0f;

        while (amountRestored < totalAmount && currentSanity < maxSanity)
        {
            float restoreThisFrame = ratePerSecond * Time.deltaTime;

            // Don't restore more than needed
            restoreThisFrame = Mathf.Min(restoreThisFrame, totalAmount - amountRestored);

            currentSanity += restoreThisFrame;
            currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
            amountRestored += restoreThisFrame;

            // Invoke the event
            OnSanityChanged?.Invoke(currentSanity, maxSanity);

            yield return null;
        }

        restoreCoroutine = null;
    }

    public void StopRestoration()
    {
        if (restoreCoroutine != null)
        {
            StopCoroutine(restoreCoroutine);
            restoreCoroutine = null;
        }
    }

    public void SetRestoreRate(float newRate)
    {
        restoreRatePerSecond = Mathf.Max(0f, newRate);
    }

    public void SetSanity(float newValue)
    {
        currentSanity = Mathf.Clamp(newValue, 0f, maxSanity);
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    public void SetSanityValues(float newCurrent, float newMax)
    {
        maxSanity = Mathf.Max(0f, newMax);
        currentSanity = Mathf.Clamp(newCurrent, 0f, maxSanity);
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    //Function so that there is no breakage when camera updates
    private void FindPlayerCamera()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera != null)
                originalLocalRot = playerCamera.transform.localRotation;
        }
    }

    private void LateUpdate()
    {
        ApplyCameraShake();
    }

    //Function to apply the effect on playerCamera
    private void ApplyCameraShake()
    {
        if (playerCamera == null)
            return;

        if (shakeTime > 0)
        {

            float x = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            float y = Mathf.Cos(Time.time * shakeSpeed * 0.9f) * shakeAmount;

            Quaternion shakeRot = Quaternion.Euler(x, y, 0f);
            playerCamera.transform.localRotation = Quaternion.Slerp(
                playerCamera.transform.localRotation,
                playerCamera.transform.localRotation * shakeRot,
                Time.deltaTime * 20f
            );

            shakeTime -= Time.deltaTime;
        }
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

        //Temporary
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = Mathf.Lerp(1f, 0f, sanityPercent);
        }

        //Temporary
        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = Mathf.Lerp(-0.5f, 0f, sanityPercent);
        }

        //Temporary
        if (motionBlur != null)
        {
            motionBlur.intensity.value = Mathf.Lerp(0.5f, 0f, sanityPercent);
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

        if (sanityPercent > 0.5f)
        {
            foreach (Light light in lightsToFlicker)
            {
                if (light == null) continue;
                light.intensity = 1f;
            }
            return;
        }
        float minInterval = Mathf.Lerp(1.5f, 0.05f, 1f - sanityPercent);
        float maxInterval = Mathf.Lerp(3f, 0.3f, 1f - sanityPercent);

        for (int i = 0; i < lightsToFlicker.Length; i++)
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

    private void OnDestroy()
    {
        // Clean up coroutine
        if (restoreCoroutine != null)
            StopCoroutine(restoreCoroutine);
    }
}
