using UnityEngine;

public class ArmPickupScript : MonoBehaviour, InteractableScript
{
    [Header("Pickup Settings")]
    [Tooltip("Reference to the player camera version of this object (used to play animation)")]
    public GameObject playerCameraObject;

    [Tooltip("Maximum distance player can be to pick up")]
    public float pickupDistance = 3f;

    [Tooltip("Tag used to identify the player")]
    public string playerTag = "Player";

    private Transform player;
    private bool isPlayerNear = false;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("PickupItem: No object found with tag '" + playerTag + "'");
        }
    }

    public void InteractScript()
    {
        Pickup();
        StartingRoomPickupManager.Instance?.ItemPickedUp(gameObject);
    }

    void Pickup()
    {
        // Trigger animation on player's camera object
        if (playerCameraObject != null)
        {
            Animator anim = playerCameraObject.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.SetBool("PickedUp", true);
            }
        }
        DialogueTrigger dialogue = GetComponent<DialogueTrigger>();
         if (dialogue != null)
        {
            dialogue.TriggerNow();
        }
        // Hide this object in the world
        gameObject.SetActive(false);

        Debug.Log($"{gameObject.name} picked up!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}
