using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //  HUD subscribers will listen to this
    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action OnDeath;

    [Header("Player Settings")]
    public int MaxHealth = 100;
    public int Health = 100;

    [Header("Health Regeneration")]
    [Tooltip("Enable health regeneration")]
    public bool enableRegen = true;
    [Tooltip("Health points restored per second")]
    public float regenRate = 20f;
    [Tooltip("Delay in seconds after taking damage before regeneration starts")]
    public float regenDelay = 1f;

    [Header("UI References")]
    public Image bloodyScreenImage;

    private bool isShowingBlood = false;
    private Coroutine bloodCR;
    private Coroutine regenCR;
    private float lastDamageTime;

    private void Start()
    {
        // Clamp & sync starting values
        Health = Mathf.Clamp(Health, 0, MaxHealth);
        OnHealthChanged?.Invoke(Health, MaxHealth);

        // Ensure the bloody screen starts invisible
        if (bloodyScreenImage != null)
        {
            Color color = bloodyScreenImage.color;
            color.a = 0f;
            bloodyScreenImage.color = color;
            bloodyScreenImage.enabled = false;
        }

        // Start regeneration coroutine if enabled
        if (enableRegen)
        {
            regenCR = StartCoroutine(RegenerateHealth());
        }
    }

    public void TakeDamage(int damage)
    {
        if (Health <= 0) return;

        // Subtract & clamp
        Health = Mathf.Clamp(Health - Mathf.Max(0, damage), 0, MaxHealth);
        Debug.Log($"Player took {damage} damage. Remaining Health: {Health}");

        // Record time of damage for regeneration delay
        lastDamageTime = Time.time;

        // Notify HUD
        OnHealthChanged?.Invoke(Health, MaxHealth);

        // Trigger bloody screen effect
        if (bloodCR != null) StopCoroutine(bloodCR);
        bloodCR = StartCoroutine(BloodyScreenEffect());

        if (Health <= 0)
            PlayerDeath();
    }

    public void Heal(int amount)
    {
        if (Health <= 0) return;

        Health = Mathf.Clamp(Health + Mathf.Max(0, amount), 0, MaxHealth);
        OnHealthChanged?.Invoke(Health, MaxHealth);
    }

    private IEnumerator RegenerateHealth()
    {
        float accumulatedRegen = 0f;

        while (true)
        {
            yield return null;

            if (enableRegen && Health > 0 && Health < MaxHealth)
            {
                float timeSinceLastDamage = Time.time - lastDamageTime;

                if (timeSinceLastDamage >= regenDelay)
                {
                    // Accumulate regeneration
                    accumulatedRegen += regenRate * Time.deltaTime;

                    // Apply when we've accumulated at least 1 HP
                    if (accumulatedRegen >= 1f)
                    {
                        int hpToAdd = Mathf.FloorToInt(accumulatedRegen);
                        accumulatedRegen -= hpToAdd;

                        Health = Mathf.Clamp(Health + hpToAdd, 0, MaxHealth);
                        OnHealthChanged?.Invoke(Health, MaxHealth);
                    }
                }
                else
                {
                    // Reset accumulated regen while waiting
                    accumulatedRegen = 0f;
                }
            }
            else
            {
                // Reset accumulated regen
                accumulatedRegen = 0f;
            }
        }
    }

    public void SetRegenerationEnabled(bool enabled)
    {
        enableRegen = enabled;

        if (enabled && regenCR == null)
        {
            regenCR = StartCoroutine(RegenerateHealth());
        }
        else if (!enabled && regenCR != null)
        {
            StopCoroutine(regenCR);
            regenCR = null;
        }
    }

    public void SetRegenRate(float newRate)
    {
        regenRate = Mathf.Max(0f, newRate);
    }

    public void SetRegenDelay(float newDelay)
    {
        regenDelay = Mathf.Max(0f, newDelay);
    }

    private void PlayerDeath()
    {
        var fps = GetComponent<SimpleFPS>();
        if (fps != null) fps.enabled = false;

        // Stop regeneration on death
        if (regenCR != null)
        {
            StopCoroutine(regenCR);
            regenCR = null;
        }

        Debug.Log("Player died");

        // Trigger the death event
        OnDeath?.Invoke();
    }

    private IEnumerator BloodyScreenEffect()
    {
        if (bloodyScreenImage == null) yield break;

        isShowingBlood = true;
        bloodyScreenImage.enabled = true;

        // Fade in
        float fadeInTime = 0.1f;
        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
            var c = bloodyScreenImage.color; c.a = alpha; bloodyScreenImage.color = c;
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        // Fade out
        float fadeOutTime = 1.5f;
        elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
            var c = bloodyScreenImage.color; c.a = alpha; bloodyScreenImage.color = c;
            elapsed += Time.deltaTime;
            yield return null;
        }

        bloodyScreenImage.enabled = false;
        isShowingBlood = false;
        bloodCR = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.gameObject.GetComponent<TestEnemy>();
            if (enemy != null) TakeDamage(enemy.damage);
        }
    }

    private void OnDestroy()
    {
        if (bloodCR != null) StopCoroutine(bloodCR);
        if (regenCR != null) StopCoroutine(regenCR);
    }
}