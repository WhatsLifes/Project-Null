using System.Collections;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("Health Display")]
    [SerializeField] private Player player;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private CanvasGroup healthGroup;

    [Header("Objectives Display")]
    [SerializeField] private TMP_Text objectiveLabel;  // NEW: The "OBJECTIVE:" text
    [SerializeField] private TMP_Text objectiveText;   // The actual objective
    [SerializeField] private CanvasGroup objectiveGroup;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;

    private void OnEnable()
    {
        if (player != null)
            player.OnHealthChanged += UpdateHealthDisplay;
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnHealthChanged -= UpdateHealthDisplay;
    }

    private void Start()
    {
        // Hide everything at start
        if (healthGroup != null)
        {
            healthGroup.alpha = 0f;
            healthGroup.gameObject.SetActive(false);
        }

        if (objectiveGroup != null)
        {
            objectiveGroup.alpha = 0f;
            objectiveGroup.gameObject.SetActive(false);
        }
    }

    private void UpdateHealthDisplay(int current, int max)
    {
        if (healthBar != null) healthBar.value = (max > 0) ? (float)current / max : 0f;
        if (healthText != null) healthText.text = $"{current} / {max}";
    }

    // ===== PUBLIC FUNCTIONS TO CALL FROM OTHER SCRIPTS =====

    public void ShowHealthBar()
    {
        if (healthGroup != null)
        {
            healthGroup.gameObject.SetActive(true);
            StartCoroutine(FadeInCanvasGroup(healthGroup));

            if (player != null)
                UpdateHealthDisplay(player.Health, player.MaxHealth);
        }
    }

    public void HideHealthBar()
    {
        if (healthGroup != null)
            StartCoroutine(FadeOutCanvasGroup(healthGroup));
    }

    /// <summary>
    /// Call this from your cutscene script to show the first objective
    /// </summary>
    public void ShowObjective1()
    {
        ShowObjective("Look around the room");  // CHANGED: No "Objective:" prefix
    }

    /// <summary>
    /// Call this when the player triggers the second objective
    /// </summary>
    public void ShowObjective2()
    {
        ShowObjective("Explore the laboratory");  // CHANGED: No "Objective:" prefix
    }

    /// <summary>
    /// Call this when the player triggers the third objective
    /// </summary>
    public void ShowObjective3()
    {
        ShowObjective("Investigate the Doll Room");  // CHANGED: No "Objective:" prefix
    }

    /// <summary>
    /// Generic function to show any objective text
    /// </summary>
    public void ShowObjective(string objectiveMessage)
    {
        if (objectiveText != null)
            objectiveText.text = objectiveMessage;  // Just the objective, no "Objective:" prefix

        if (objectiveGroup != null)
        {
            objectiveGroup.gameObject.SetActive(true);
            StartCoroutine(FadeInCanvasGroup(objectiveGroup));
        }
    }

    /// <summary>
    /// Call this to hide the objective display
    /// </summary>
    public void HideObjective()
    {
        if (objectiveGroup != null)
            StartCoroutine(FadeOutCanvasGroup(objectiveGroup));
    }

    /// <summary>
    /// Call this to update the objective text without fading
    /// </summary>
    public void UpdateObjective(string objectiveMessage)
    {
        if (objectiveText != null)
            objectiveText.text = objectiveMessage;
    }

    // ===== HELPER FUNCTIONS =====

    private IEnumerator FadeInCanvasGroup(CanvasGroup group)
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }
        group.alpha = 1f;
    }

    private IEnumerator FadeOutCanvasGroup(CanvasGroup group)
    {
        float elapsed = 0f;
        float startAlpha = group.alpha;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeInDuration);
            yield return null;
        }

        group.alpha = 0f;
        group.gameObject.SetActive(false);
    }

    // ===== OPTIONAL: PLAYER SWAPPING =====

    public void SetPlayer(Player newPlayer)
    {
        if (player != null)
            player.OnHealthChanged -= UpdateHealthDisplay;

        player = newPlayer;

        if (player != null && isActiveAndEnabled)
        {
            player.OnHealthChanged += UpdateHealthDisplay;
            UpdateHealthDisplay(player.Health, player.MaxHealth);
        }
    }
}
