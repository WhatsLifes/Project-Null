using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;

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
    [SerializeField] private CanvasGroup syringeGroup;
    [SerializeField] private CanvasGroup inventoryGroup;
    [SerializeField] private Inventory inventory;

    [Header("Sanity Display")]
    [SerializeField] private Sanity sanity;
    [SerializeField] private Slider sanityBar;
    [SerializeField] private CanvasGroup sanityGroup;

    [Header("Timer Display")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text timerTitleText;
    [SerializeField] private CanvasGroup timerGroup;
    [SerializeField] private StageTimer stageTimer;

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

        if (timerGroup != null)
        {
            timerGroup.alpha = 0f;
            timerGroup.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        HandleBatteryDisplay();
        HandleInventoryDisplay();
        HandleTimerDisplay();
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
    public void ShowObjective6() => ShowObjective("Identify the voice");
    public void ShowObjective7() => ShowObjective("Explore the Carnival Room");
    public void ShowObjective8() => ShowObjective("Locate the next picture piece");
    public void ShowObjective9() => ShowObjective("Find a way to the next floor");
    public void ShowObjective10() => ShowObjective("Pick up the flowers (0/2)");
    public void ShowObjective11() => ShowObjective("Pick up the flowers (1/2)");
    public void ShowObjective12() => ShowObjective("Continue to the next floor");
    public void ShowObjective13() => ShowObjective("Find the last flower");
    public void ShowObjective14() => ShowObjective("Make the Tri-Flora Compound");
    public void ShowObjective15() => ShowObjective("Turn on the generators (0/2)");
    public void ShowObjective16() => ShowObjective("Turn on the generators (1/2)");
    public void ShowObjective17() => ShowObjective("Return to the workstation & try again");

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

        if (flashlightToggle.isPickedUp)
        {
            if (!batteryGroup.gameObject.activeSelf)
            {
                batteryGroup.gameObject.SetActive(true);
                StartCoroutine(FadeInCanvasGroup(batteryGroup));
            }

            float percent = Mathf.Clamp01(flashlightToggle.currentBattery / flashlightToggle.maxBattery) * 100f;
            int roundedPercent = Mathf.RoundToInt(percent / 5f) * 5;
            batteryText.text = $"Battery: {roundedPercent}%";
        }
        else
        {
            if (batteryGroup.gameObject.activeSelf)
                StartCoroutine(FadeOutCanvasGroup(batteryGroup));
        }
    }

    // ===== INVENTORY DISPLAY =====

    public void ShowInventorySlot()
    {
        if (inventoryGroup != null)
        {
            inventoryGroup.gameObject.SetActive(true);
            StartCoroutine(FadeInCanvasGroup(inventoryGroup));
        }
    }

    public void HideInventorySlot()
    {
        if (inventoryGroup != null)
            StartCoroutine(FadeOutCanvasGroup(inventoryGroup));
    }

    public void ShowInventoryDisplay()
    {
        ShowInventorySlot();
    }

    public void HideInventoryDisplay()
    {
        HideInventorySlot();
    }

    public void HandleInventoryDisplay()
    {
        if (inventory == null || syringeGroup == null || syringeImage == null) return;

        if (inventory.holdingSyringe)
        {
            if (!syringeGroup.gameObject.activeSelf)
            {
                syringeGroup.gameObject.SetActive(true);
                StartCoroutine(FadeInCanvasGroup(syringeGroup));
            }
        }
        else
        {
            if (syringeGroup.gameObject.activeSelf)
                StartCoroutine(FadeOutCanvasGroup(syringeGroup));
        }
    }

    // ===== TIMER DISPLAY HANDLING =====

    public void ShowTimer()
    {
        if (timerGroup != null)
        {
            timerGroup.gameObject.SetActive(true);
            StartCoroutine(FadeInCanvasGroup(timerGroup));
        }
    }

    public void HideTimer()
    {
        if (timerGroup != null)
            StartCoroutine(FadeOutCanvasGroup(timerGroup));
    }

    public void HandleTimerDisplay()
    {
        if (stageTimer == null || timerText == null || timerGroup == null)
            return;

        // Show timer if it's active
        if (stageTimer.IsTimerActive())
        {
            if (!timerGroup.gameObject.activeSelf)
            {
                timerGroup.gameObject.SetActive(true);
                StartCoroutine(FadeInCanvasGroup(timerGroup));

                // Set timer title (only needs to be set once)
                if (timerTitleText != null)
                {
                    timerTitleText.text = "POISON TIMER";
                }
            }

            // Update timer text
            float timeRemaining = stageTimer.GetTimeRemaining();
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);

            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Color code based on time remaining
            if (timeRemaining <= 10f)
            {
                timerText.color = Color.red;
            }
            else if (timeRemaining <= 60f)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
        else
        {
            // Hide timer if not active
            if (timerGroup.gameObject.activeSelf)
                StartCoroutine(FadeOutCanvasGroup(timerGroup));
        }
    }

    // ===== HIDE ALL HUD ELEMENTS =====
    public void HideAllHUD()
    {
        if (healthGroup != null)
        {
            healthGroup.alpha = 0f;
            healthGroup.gameObject.SetActive(false);
        }

        if (sanityGroup != null)
        {
            sanityGroup.alpha = 0f;
            sanityGroup.gameObject.SetActive(false);
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

        if (timerGroup != null)
        {
            timerGroup.alpha = 0f;
            timerGroup.gameObject.SetActive(false);
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