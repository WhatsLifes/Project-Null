using System.Collections;
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
    [SerializeField] private TMP_Text objectiveLabel;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private CanvasGroup objectiveGroup;

    [Header("Battery Display")]
    [SerializeField] private TMP_Text batteryText;
    [SerializeField] private CanvasGroup batteryGroup;
    [SerializeField] private FlashlightToggle flashlightToggle;

    [Header("Inventory Display")]
    [SerializeField] private Image syringeImage;
    [SerializeField] private CanvasGroup syringeGroup; // NEW: Separate group for just the syringe image
    [SerializeField] private CanvasGroup inventoryGroup; // This is for the slot (border, background, text)
    [SerializeField] private Inventory inventory;

    [Header("Sanity Display")]
    [SerializeField] private Sanity sanity;
    [SerializeField] private Slider sanityBar;
    [SerializeField] private CanvasGroup sanityGroup;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;

    private void OnEnable()
    {
        if (player != null)
            player.OnHealthChanged += UpdateHealthDisplay;

        if (sanity != null)
            sanity.OnSanityChanged += UpdateSanityDisplay;
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnHealthChanged -= UpdateHealthDisplay;

        if (sanity != null)
            sanity.OnSanityChanged -= UpdateSanityDisplay;
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

        if (batteryGroup != null)
        {
            batteryGroup.alpha = 0f;
            batteryGroup.gameObject.SetActive(false);
        }

        // Hide both inventory slot and syringe at start
        if (inventoryGroup != null)
        {
            inventoryGroup.alpha = 0f;
            inventoryGroup.gameObject.SetActive(false);
        }

        if (syringeGroup != null)
        {
            syringeGroup.alpha = 0f;
            syringeGroup.gameObject.SetActive(false);
        }

        if (sanityGroup != null)
        {
            sanityGroup.alpha = 0f;
            sanityGroup.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        HandleBatteryDisplay();
        HandleInventoryDisplay();
    }

    private void UpdateHealthDisplay(int current, int max)
    {
        if (healthBar != null) healthBar.value = (max > 0) ? (float)current / max : 0f;
        if (healthText != null) healthText.text = $"{current} / {max}";
    }

    private void UpdateSanityDisplay(float current, float max)
    {
        if (sanityBar != null) sanityBar.value = (max > 0f) ? current / max : 0f;
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

    public void ShowSanityBar()
    {
        if (sanityGroup != null)
        {
            sanityGroup.gameObject.SetActive(true);
            StartCoroutine(FadeInCanvasGroup(sanityGroup));
            if (player != null)
                UpdateSanityDisplay(sanity.currentSanity, sanity.maxSanity);
        }
    }

    public void HideHealthBar()
    {
        if (healthGroup != null)
            StartCoroutine(FadeOutCanvasGroup(healthGroup));
    }

    public void HideSanityBar()
    {
        if (sanityGroup != null)
            StartCoroutine(FadeOutCanvasGroup(sanityGroup));
    }

    public void ShowObjective1() => ShowObjective("Look around the room");
    public void ShowObjective2() => ShowObjective("Explore the laboratory");
    public void ShowObjective3() => ShowObjective("Investigate the Doll Room");
    public void ShowObjective4() => ShowObjective("Solve her puzzle");
    public void ShowObjective5() => ShowObjective("Find a way to the second floor");
    public void ShowObjective6() => ShowObjective("Figure out who the voice is");
    public void ShowObjective7() => ShowObjective("Explore the Carnival Room");
    public void ShowObjective8() => ShowObjective("Locate the next picture piece");

    public void ShowObjective(string objectiveMessage)
    {
        if (objectiveText != null)
            objectiveText.text = objectiveMessage;

        if (objectiveGroup != null)
        {
            objectiveGroup.gameObject.SetActive(true);
            StartCoroutine(FadeInCanvasGroup(objectiveGroup));
        }
    }

    public void HideObjective()
    {
        if (objectiveGroup != null)
            StartCoroutine(FadeOutCanvasGroup(objectiveGroup));
    }

    public void UpdateObjective(string objectiveMessage)
    {
        if (objectiveText != null)
            objectiveText.text = objectiveMessage;
    }

    // ===== BATTERY DISPLAY HANDLING =====

    public void ShowBatteryDisplay()
    {
        if (batteryGroup != null && flashlightToggle != null && batteryText != null)
        {
            batteryGroup.gameObject.SetActive(true);
            StartCoroutine(FadeInCanvasGroup(batteryGroup));

            // Update the text to show current battery percentage in intervals of 5
            float percent = Mathf.Clamp01(flashlightToggle.currentBattery / flashlightToggle.maxBattery) * 100f;
            int roundedPercent = Mathf.RoundToInt(percent / 5f) * 5;
            batteryText.text = $"Battery: {roundedPercent}%";
        }
    }

    public void HideBatteryDisplay()
    {
        if (batteryGroup != null)
            StartCoroutine(FadeOutCanvasGroup(batteryGroup));
    }

    public void HandleBatteryDisplay()
    {
        if (flashlightToggle == null || batteryText == null || batteryGroup == null)
            return;

        // Only show battery display if flashlight is picked up
        if (flashlightToggle.isPickedUp)
        {
            if (!batteryGroup.gameObject.activeSelf)
            {
                batteryGroup.gameObject.SetActive(true);
                StartCoroutine(FadeInCanvasGroup(batteryGroup));
            }

            // Update the text to show current battery percentage in intervals of 5
            float percent = Mathf.Clamp01(flashlightToggle.currentBattery / flashlightToggle.maxBattery) * 100f;
            int roundedPercent = Mathf.RoundToInt(percent / 5f) * 5;
            batteryText.text = $"Battery: {roundedPercent}%";
        }
        else
        {
            // Hide the battery display if flashlight not yet picked up
            if (batteryGroup.gameObject.activeSelf)
                StartCoroutine(FadeOutCanvasGroup(batteryGroup));
        }
    }

    // =====    INVENTORY DISPLAY      =====

    // NEW: Show the inventory slot (border, background, text) - call this early in the game
    public void ShowInventorySlot()
    {
        if (inventoryGroup != null)
        {
            inventoryGroup.gameObject.SetActive(true);
            StartCoroutine(FadeInCanvasGroup(inventoryGroup));
        }
    }

    // NEW: Hide the inventory slot if needed
    public void HideInventorySlot()
    {
        if (inventoryGroup != null)
            StartCoroutine(FadeOutCanvasGroup(inventoryGroup));
    }

    // DEPRECATED: Kept for backwards compatibility, but consider using ShowInventorySlot instead
    public void ShowInventoryDisplay()
    {
        ShowInventorySlot();
    }

    // DEPRECATED: Kept for backwards compatibility
    public void HideInventoryDisplay()
    {
        HideInventorySlot();
    }

    // UPDATED: Now handles only the syringe image fading in/out
    public void HandleInventoryDisplay()
    {
        if (inventory == null || syringeGroup == null || syringeImage == null) return;

        if (inventory.holdingSyringe)
        {
            // Show the syringe image
            if (!syringeGroup.gameObject.activeSelf)
            {
                syringeGroup.gameObject.SetActive(true);
                StartCoroutine(FadeInCanvasGroup(syringeGroup));
            }
        }
        else
        {
            // Hide the syringe image
            if (syringeGroup.gameObject.activeSelf)
                StartCoroutine(FadeOutCanvasGroup(syringeGroup));
        }
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
