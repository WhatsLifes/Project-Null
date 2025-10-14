using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    public int Health = 100;

    [Header("UI References")]
    public Image bloodyScreenImage;  // Assign your bloody PNG Image here (in Canvas)

    private bool isShowingBlood = false;

    private void Start()
    {
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
        if (Health <= 0)
            return;

        // Subtract health
        Health -= damage;
        Debug.Log($"Player took {damage} damage. Remaining Health: {Health}");

        // Trigger bloody screen effect
        StartCoroutine(BloodyScreenEffect());

        // If health reaches zero
        if (Health <= 0)
            PlayerDeath();
    }

    private void PlayerDeath()
    {
        // Disable player movement (if you have a movement script)
        var fps = GetComponent<SimpleFPS>();
        if (fps != null)
            fps.enabled = false;

        Debug.Log("Player died");
    }

    private IEnumerator BloodyScreenEffect()
    {
        if (bloodyScreenImage == null)
            yield break;

        isShowingBlood = true;
        bloodyScreenImage.enabled = true;

        // Fade in quickly to full alpha (show blood)
        float fadeInTime = 0.1f;
        float elapsed = 0f;

        while (elapsed < fadeInTime)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
            Color color = bloodyScreenImage.color;
            color.a = alpha;
            bloodyScreenImage.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Hold briefly before fading out
        yield return new WaitForSeconds(0.3f);

        // Fade out smoothly
        float fadeOutTime = 1.5f;
        elapsed = 0f;

        while (elapsed < fadeOutTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
            Color color = bloodyScreenImage.color;
            color.a = alpha;
            bloodyScreenImage.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }

        bloodyScreenImage.enabled = false;
        isShowingBlood = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Apply damage from enemy
            TakeDamage(other.gameObject.GetComponent<TestEnemy>().damage);
        }
    }
}
