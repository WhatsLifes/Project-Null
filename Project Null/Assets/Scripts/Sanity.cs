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
    public Light playerLight;

    private float halfSanityThreshold;
    private float passiveDrainTimer = 0f;

    private void Start()
    {
        currentSanity = 0f;
        halfSanityThreshold = maxSanity / 2f;

        RestoreSanity(maxSanity);
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
    }

    public void TakeSanityDamage(float amount)
    {
        currentSanity += amount;
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

        if(playerCamera != null)
        {
            playerCamera.fieldOfView = Mathf.Lerp(40f, 60f, sanityPercent);
        }

        if(playerLight != null)
        {
            float baseIntensity = 1f;
            float flickerStrength = (1f - sanityPercent) * 0.3f;
            float noise = Mathf.PerlinNoise(Time.time * 10f, 0f) * flickerStrength;

            playerLight.intensity = baseIntensity + noise;
        }

        if(ringingAudio != null)
        {
            ringingAudio.volume = 1f - sanityPercent;
        }

        if(sanityPercent <= 0.5f)
        {
            if(Random.value < 0.01f)
            {
                Debug.Log("You hear footseps around you...");
            }
        }
    }
}
