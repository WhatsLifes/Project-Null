using UnityEngine;

public class ArmPickupScript : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("Reference to the player camera version of this object (used to play animation)")]
    public GameObject playerCameraObject;

    [Tooltip("Key to press for pickup")]
    public KeyCode pickupKey = KeyCode.E;

    [Tooltip("Maximum distance player can be to pick up")]
    public float pickupDistance = 3f;

    [Tooltip("Tag used to identify the player")]
    public string playerTag = "Player";

    [Header("Animation Settings")]
    [Tooltip("Name of the bool parameter in the Animator to trigger pickup animation")]
    public string pickupBoolName = "PickedUp";

    [Header("UI Settings")]
    [Tooltip("Show pickup prompt when near")]
    public bool showPickupPrompt = true;

    [Tooltip("Text displayed when player is in range")]
    public string pickupPromptText = "Press E to attach arm";

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

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        isPlayerNear = distance <= pickupDistance;

        if (isPlayerNear && Input.GetKeyDown(pickupKey))
        {
            Pickup();
        }
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

    void OnGUI()
    {
        if (showPickupPrompt && isPlayerNear)
        {
            float width = 300f;
            float height = 30f;
            float x = (Screen.width - width) / 2f;
            float y = Screen.height - 100f;

            GUI.Label(new Rect(x, y, width, height), pickupPromptText);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}
