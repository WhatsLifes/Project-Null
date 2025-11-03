using UnityEngine;

public class FlashlightPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("The FlashlightToggle script to enable when picked up")]
    public FlashlightToggle flashlightToggleScript;

    [Tooltip("Key to press to pick up flashlight")]
    public KeyCode pickupKey = KeyCode.E;

    [Tooltip("Maximum distance player can be to pick up")]
    public float pickupDistance = 3f;

    [Tooltip("Tag of the player (to detect proximity)")]
    public string playerTag = "Player";

    [Header("UI Settings")]
    [Tooltip("Show pickup prompt when near")]
    public bool showPickupPrompt = true;

    [Tooltip("Text to display for pickup prompt")]
    public string pickupPromptText = "Press E to pick up Flashlight";

    private Transform player;
    private bool isPlayerNear = false;

    void Start()
    {
        // Find the player in the scene
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("FlashlightPickup: No object with tag '" + playerTag + "' found!");
        }
    }

    void Update()
    {
        if (player == null) return;

        // Check distance to player
        float distance = Vector3.Distance(transform.position, player.position);
        isPlayerNear = distance <= pickupDistance;

        // Handle pickup
        if (isPlayerNear && Input.GetKeyDown(pickupKey))
        {
            PickupFlashlight();
        }
    }

    void PickupFlashlight()
    {
        // Enable the flashlight toggle script
        if (flashlightToggleScript != null)
        {
            flashlightToggleScript.Pickup();
            StartingRoomPickupManager.Instance?.ItemPickedUp(gameObject);

        }

        // Make this object disappear
        gameObject.SetActive(false);

        Debug.Log("Flashlight picked up!");
    }

    void OnGUI()
    {
        if (showPickupPrompt && isPlayerNear)
        {
            // Center the text at the bottom of the screen
            float width = 300f;
            float height = 30f;
            float x = (Screen.width - width) / 2f;
            float y = Screen.height - 100f;

            GUI.Label(new Rect(x, y, width, height), pickupPromptText);
        }
    }

    // Optional: Draw the pickup radius in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}