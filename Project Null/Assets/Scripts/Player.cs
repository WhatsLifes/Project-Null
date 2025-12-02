using System; //  add this
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //  HUD subscribers will listen to this
    public event Action<int, int> OnHealthChanged; // (current, max)

    [Header("Player Settings")]
    public int MaxHealth = 100;
    public int Health = 100;

    [Header("Health Regeneration")]
    [Tooltip("Enable health regeneration")]
    public bool enableRegen = true;
    [Tooltip("Health points restored per second")]
    public float regenRate = 5f;
    [Tooltip("Delay in seconds after taking damage before regeneration starts")]
    public float regenDelay = 3f;

    [Header("UI References")]
    public Image bloodyScreenImage;  // Assign your bloody PNG Image here (in Canvas)

    private bool isShowingBlood = false;
    private Coroutine bloodCR; // prevent overlapping fades
    private Coroutine regenCR; // regeneration coroutine
    private float lastDamageTime; // track when damage was last taken

    private void Start()
    {
        // Clamp & sync starting values
        Health = Mathf.Clamp(Health, 0, MaxHealth);
        OnHealthChanged?.Invoke(Health, MaxHealth); //  notify HUD at start

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

        //  notify HUD
        OnHealthChanged?.Invoke(Health, MaxHealth);

        // Trigger bloody screen effect (cancel previous if still running)
        if (bloodCR != null) StopCoroutine(bloodCR);
        bloodCR = StartCoroutine(BloodyScreenEffect());

        if (Health <= 0)
            PlayerDeath();
    }

    // Optional: healing support
    public void Heal(int amount)
    {
        if (Health <= 0) return;

        Health = Mathf.Clamp(Health + Mathf.Max(0, amount), 0, MaxHealth);
        OnHealthChanged?.Invoke(Health, MaxHealth);
    }

    private IEnumerator RegenerateHealth()
    {
        while (true)
        {
            yield return null; // Wait one frame

            // Only regenerate if:
            // 1. Regen is enabled
            // 2. Player is alive
            // 3. Health is not full
            // 4. Enough time has passed since last damage
            if (enableRegen && Health > 0 && Health < MaxHealth)
            {
                float timeSinceLastDamage = Time.time - lastDamageTime;

                if (timeSinceLastDamage >= regenDelay)
                {
                    // Regenerate health
                    float regenAmount = regenRate * Time.deltaTime;
                    int previousHealth = Health;

                    // Use float for precise calculation, then convert to int
                    float newHealth = Health + regenAmount;
                    Health = Mathf.Clamp(Mathf.RoundToInt(newHealth), 0, MaxHealth);

                    // Only notify HUD if health actually changed
                    if (Health != previousHealth)
                    {
                        OnHealthChanged?.Invoke(Health, MaxHealth);
                    }
                }
            }
        }
    }

    // Public method to toggle regeneration on/off at runtime
    public void SetRegenerationEnabled(bool enabled)
    {
        enableRegen = enabled;

        // Start or stop the coroutine based on the new state
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

    // Public method to adjust regen rate at runtime
    public void SetRegenRate(float newRate)
    {
        regenRate = Mathf.Max(0f, newRate); // Ensure it's not negative
    }

    // Public method to adjust regen delay at runtime
    public void SetRegenDelay(float newDelay)
    {
        regenDelay = Mathf.Max(0f, newDelay); // Ensure it's not negative
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
        // Clean up coroutines when destroyed
        if (bloodCR != null) StopCoroutine(bloodCR);
        if (regenCR != null) StopCoroutine(regenCR);
    }
}