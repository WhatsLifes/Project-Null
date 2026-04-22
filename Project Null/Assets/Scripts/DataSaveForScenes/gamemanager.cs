using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int playerHealth = 100;
    public int playerMaxHealth = 100;
    public float playerSanity = 100f;
    public float playerMaxSanity = 100f;
    public float flashlightBattery = 100f;
    public float flashlightMaxBattery = 100f;
    public bool flashlightPickedUp = false;
    public bool holdingSyringe = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SavePlayerState(Player player, Sanity sanity, FlashlightToggle flashlight, Inventory inventory)
    {
        if (player != null)
        {
            playerHealth = player.Health;
            playerMaxHealth = player.MaxHealth;
        }

        if (sanity != null)
        {
            playerSanity = sanity.currentSanity;
            playerMaxSanity = sanity.maxSanity;
        }

        if (flashlight != null)
        {
            flashlightBattery = flashlight.currentBattery;
            flashlightMaxBattery = flashlight.maxBattery;
            flashlightPickedUp = flashlight.isPickedUp;
        }

        if (inventory != null)
        {
            holdingSyringe = inventory.holdingSyringe;
        }
    }

    public void LoadPlayerState(Player player, Sanity sanity, FlashlightToggle flashlight, Inventory inventory)
    {
        if (player != null)
        {
            player.MaxHealth = playerMaxHealth;
            player.Health = playerHealth;
            player.Heal(0);
        }

        if (sanity != null)
        {
            sanity.SetSanityValues(playerSanity, playerMaxSanity);
        }

        if (flashlight != null)
        {
            flashlight.currentBattery = flashlightBattery;
            flashlight.maxBattery = flashlightMaxBattery;
            flashlight.isPickedUp = flashlightPickedUp;
        }

        if (inventory != null)
        {
            inventory.holdingSyringe = holdingSyringe;
        }
    }

    public void ResetGameData()
    {
        playerHealth = 100;
        playerMaxHealth = 100;
        playerSanity = 100f;
        playerMaxSanity = 100f;
        flashlightBattery = 100f;
        flashlightMaxBattery = 100f;
        flashlightPickedUp = false;
        holdingSyringe = false;
    }
}