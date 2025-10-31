using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthHUD : MonoBehaviour
{
    [SerializeField] private Player player;   // drag your Player here
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;

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
        if (player != null)
            UpdateHealthDisplay(player.Health, player.MaxHealth); // initialize once
    }

    private void UpdateHealthDisplay(int current, int max)
    {
        if (healthBar != null) healthBar.value = (max > 0) ? (float)current / max : 0f;
        if (healthText != null) healthText.text = $"{current} / {max}";
    }

    // Optional: if you ever swap players (cutscene/respawn)
    public void SetPlayer(Player newPlayer)
    {
        if (player != null) player.OnHealthChanged -= UpdateHealthDisplay;
        player = newPlayer;
        if (isActiveAndEnabled && player != null)
            player.OnHealthChanged += UpdateHealthDisplay;
        if (player != null)
            UpdateHealthDisplay(player.Health, player.MaxHealth);
    }
}
