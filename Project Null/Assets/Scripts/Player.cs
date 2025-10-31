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

    [Header("UI References")]
    public Image bloodyScreenImage;  // Assign your bloody PNG Image here (in Canvas)

    private bool isShowingBlood = false;
    private Coroutine bloodCR; // prevent overlapping fades

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
    }

    public void TakeDamage(int damage)
    {
        if (Health <= 0) return;

        // Subtract & clamp
        Health = Mathf.Clamp(Health - Mathf.Max(0, damage), 0, MaxHealth);
        Debug.Log($"Player took {damage} damage. Remaining Health: {Health}");

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
        OnHealthChanged?.Invoke(Health, MaxHealth); // 
    }

    private void PlayerDeath()
    {
        var fps = GetComponent<SimpleFPS>();
        if (fps != null) fps.enabled = false;
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
}
