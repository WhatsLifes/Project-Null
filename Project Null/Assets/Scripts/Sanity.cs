using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class Sanity : MonoBehaviour
{
    public static Sanity Instance;

    [Header("Sanity Settings")]
    public float maxSanity = 100f;
    public float currentSanity = 100f;
    public float passiveDrainPerMinute = 5f;

    [Header("Sanity Restoration")]
    [Tooltip("Rate at which sanity restores per second when RestoreSanity is called")]
    public float restoreRatePerSecond = 5f;

    [Header("Effects")]
    public Camera playerCamera;
    public Light[] lightsToFlicker;

    private float passiveDrainTimer = 0f;
    private float[] flickerTimers;
    private bool[] lightStates;
    private float randomSoundTimer = 0f;
    private float ringingTimer = 0f;
    private Coroutine restoreCoroutine;

    [Header("Audio")]
    public AudioSource ringingAudioSource;
    public AudioSource sanityAudioSource;
    public AudioClip ringingClip;
    public AudioClip[] randomSanityClips;

    [Header("Post Processing")]
    public Volume postProcessVolume;

    [Header("Camera Shake")]
    public float shakeAmount = 15f;
    public float shakeSpeed = 30f;

    private float shakeTime;
    private Quaternion originalLocalRot;

    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private MotionBlur motionBlur;

    public event System.Action<float, float> OnSanityChanged;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
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

        if (ringingAudioSource != null)
        {
            ringingAudioSource.spatialBlend = 0f;
            ringingAudioSource.loop = false;
            ringingAudioSource.playOnAwake = false;
            ringingAudioSource.volume = 0.2f;

            if (ringingClip != null)
                ringingAudioSource.clip = ringingClip;
        }

        if (sanityAudioSource != null)
        {
            sanityAudioSource.playOnAwake = false;
            sanityAudioSource.spatialBlend = 0f;
            sanityAudioSource.loop = false;
        }

        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out chromaticAberration);
            postProcessVolume.profile.TryGet(out lensDistortion);
            postProcessVolume.profile.TryGet(out motionBlur);
        }

        if (playerCamera != null)
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

    public void ShakeCamera(float duration, float intensity)
    {
        if (playerCamera == null)
            return;

        shakeTime = Mathf.Max(shakeTime, duration);
        shakeAmount = Mathf.Max(shakeAmount, intensity);
    }

    public void TakeSanityDamage(float amount, bool playShake = true)
    {
        if (restoreCoroutine != null)
        {
            StopCoroutine(restoreCoroutine);
            restoreCoroutine = null;
        }

        currentSanity -= amount;
        currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);

        if (playShake)
            ShakeCamera(0.25f, 0.75f);

        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    public void RestoreSanity(float amount)
    {
        RestoreSanity(amount, restoreRatePerSecond);
    }

    public void RestoreSanity(float amount, float ratePerSecond)
    {
        if (restoreCoroutine != null)
            StopCoroutine(restoreCoroutine);

        restoreCoroutine = StartCoroutine(RestoreSanityOverTime(amount, ratePerSecond));
    }

    public void RestoreSanityInstant(float amount)
    {
        currentSanity += amount;
        currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);

        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    private IEnumerator RestoreSanityOverTime(float totalAmount, float ratePerSecond)
    {
        float amountRestored = 0f;

        while (amountRestored < totalAmount && currentSanity < maxSanity)
        {
            float restoreThisFrame = ratePerSecond * Time.deltaTime;
            restoreThisFrame = Mathf.Min(restoreThisFrame, totalAmount - amountRestored);

            currentSanity += restoreThisFrame;
            currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
            amountRestored += restoreThisFrame;

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

        if (ringingAudioSource != null && ringingClip != null)
        {
            ringingTimer -= Time.deltaTime;

            if (sanityPercent < 0.5f && !ringingAudioSource.isPlaying && ringingTimer <= 0f)
            {
                float volume = Mathf.Lerp(0.1f, 0.8f, 1f - sanityPercent);
                ringingAudioSource.volume = volume;
                ringingAudioSource.panStereo = Random.Range(-0.5f, 0.5f);

                ringingAudioSource.Play();

                ringingTimer = Random.Range(
                    Mathf.Lerp(30f, 8f, 1f - sanityPercent),
                    Mathf.Lerp(60f, 15f, 1f - sanityPercent)
                );
            }
        }

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = Mathf.Lerp(1f, 0f, sanityPercent);

        if (lensDistortion != null)
            lensDistortion.intensity.value = Mathf.Lerp(-0.5f, 0f, sanityPercent);

        if (motionBlur != null)
            motionBlur.intensity.value = Mathf.Lerp(0.5f, 0f, sanityPercent);
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
        if (restoreCoroutine != null)
            StopCoroutine(restoreCoroutine);
    }
}