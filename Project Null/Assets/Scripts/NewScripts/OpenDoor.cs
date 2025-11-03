using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [Header("Reference to Door Pivot")]
    public DoorController door; // Drag DoorPivot here

    [Header("Interaction Settings")]
    public float interactDistance = 3f; // Player must be this close
    public KeyCode interactKey = KeyCode.E;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null || door == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactDistance && Input.GetKeyDown(interactKey))
        {
            door.OpenDoor(); // Rotate to 180°
            Debug.Log("Button pressed - door opening!");
            GameProgressManager.Instance.buttonPressed = true;
            Debug.Log("Progress updated: buttonPressed = true");
        }
    }
}
