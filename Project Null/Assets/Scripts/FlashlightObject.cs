using UnityEngine;

public class FlashlightPickup : MonoBehaviour, InteractableScript
{
    [Header("Pickup Settings")]
    [Tooltip("The FlashlightToggle script to enable when picked up")]
    public FlashlightToggle flashlightToggleScript;

    [Tooltip("Maximum distance player can be to pick up")]
    public float pickupDistance = 3f;

    [Tooltip("Tag of the player (to detect proximity)")]
    public string playerTag = "Player";
    

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

    public void InteractScript()
    {
        PickupFlashlight();
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

    // Optional: Draw the pickup radius in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}