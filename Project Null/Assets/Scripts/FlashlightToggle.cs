using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    public static bool hasFlashlight = false;

    [Header("Pickup Settings")]
    public bool isPickedUp = false;

    [Header("Flashlight Settings")]
    public Light flashlight;
    public KeyCode toggleKey = KeyCode.F;

    [Header("Mannequin Freeze Settings")]
    public float freezeDistance = 20f;
    public LayerMask mannequinLayer;

    [Header("Battery Settings")]
    public float maxBattery = 100f;
    public float currentBattery = 100f;
    public float drainRate = 10f;
    public float rechargeRate = 5f;

    [Header("Battery Pickup Settings")]
    public float batteryPickupAmount = 25f;
    public bool fillToMax = false;

    [Header("Flicker Settings")]
    public float flickerThreshold = 50f;
    public float minFlickerInterval = 0.3f;
    public float maxFlickerInterval = 2f;
    public float flickerDuration = 0.08f;
    public float flickerIntensityMultiplier = 0.3f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip toggleSound;

    private bool isOn = false;
    private float nextFlickerTime = 0f;
    private float flickerEndTime = 0f;
    private float originalIntensity;
    private MannequinEnemy currentlyFrozenMannequin = null;


    void Start()
    {
        isPickedUp = hasFlashlight;

        if (flashlight == null)
            flashlight = GetComponentInChildren<Light>();

        if (flashlight != null)
        {
            flashlight.enabled = false;
            originalIntensity = flashlight.intensity;
        }

        currentBattery = maxBattery;
    }

    void Update()
    {
        if (!isPickedUp) return;

        HandleToggle();
        HandleBattery();
        HandleFlicker();
        HandleMannequinFreeze();
    }

    void HandleToggle()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (audioSource != null && toggleSound != null)
                audioSource.PlayOneShot(toggleSound);

            if (!isOn && currentBattery > 0f)
            {
                isOn = true;
                if (flashlight != null)
                    flashlight.enabled = true;
            }
            else if (isOn)
            {
                isOn = false;
                if (flashlight != null)
                    flashlight.enabled = false;

                UnfreezeMannequin();
            }
        }
    }

    void HandleBattery()
    {
        if (isOn)
        {
            currentBattery -= drainRate * Time.deltaTime;
            currentBattery = Mathf.Max(0f, currentBattery);

            if (currentBattery <= 0f)
            {
                isOn = false;
                if (flashlight != null)
                    flashlight.enabled = false;
                UnfreezeMannequin();
            }
        }
        else
        {
            currentBattery += rechargeRate * Time.deltaTime;
            currentBattery = Mathf.Min(maxBattery, currentBattery);
        }
    }

    void HandleFlicker()
    {
        if (flashlight == null) return;

        if (!isOn || currentBattery > flickerThreshold)
        {
            if (flashlight.intensity != originalIntensity)
                flashlight.intensity = originalIntensity;
            return;
        }

        if (Time.time < flickerEndTime)
        {
            flashlight.intensity = originalIntensity * flickerIntensityMultiplier;

            if (currentlyFrozenMannequin != null)
            {
                currentlyFrozenMannequin.FlashlightUnfreeze();
                currentlyFrozenMannequin = null;
            }

            return;
        }

        if (flashlight.intensity != originalIntensity)
            flashlight.intensity = originalIntensity;

        if (Time.time >= nextFlickerTime)
        {
            float batteryPercent = currentBattery / flickerThreshold;
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
        if (!isOn || flashlight == null)
        {
            UnfreezeMannequin();
            return;
        }

        Ray ray = new Ray(flashlight.transform.position, flashlight.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, freezeDistance, mannequinLayer))
        {
            MannequinEnemy mannequin = hit.collider.GetComponent<MannequinEnemy>();

            if (mannequin != null)
            {
                if (currentlyFrozenMannequin != null && currentlyFrozenMannequin != mannequin)
                {
                    currentlyFrozenMannequin.FlashlightUnfreeze();
                }

                mannequin.FlashlightFreeze();
                currentlyFrozenMannequin = mannequin;
            }
            else
            {
                UnfreezeMannequin();
            }
        }
        else
        {
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

    public void Pickup()
    {
        isPickedUp = true;
        hasFlashlight = true;
    }

    public bool RechargeBattery(float amount = -1f)
    {
        if (currentBattery >= maxBattery)
            return false;

        float rechargeAmount = (amount > 0) ? amount : batteryPickupAmount;

        if (fillToMax)
        {
            currentBattery = maxBattery;
        }
        else
        {
            currentBattery += rechargeAmount;
            currentBattery = Mathf.Min(currentBattery, maxBattery);
        }

        return true;
    }

    public float GetBatteryPercentage()
    {
        return (currentBattery / maxBattery) * 100f;
    }

    public bool IsBatteryLow(float threshold = 20f)
    {
        return currentBattery <= threshold;
    }

    public void SetBattery(float amount)
    {
        currentBattery = Mathf.Clamp(amount, 0f, maxBattery);
    }
}