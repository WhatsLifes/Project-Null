using UnityEngine;

public class DoorCloseTrigger : MonoBehaviour
{
    [Header("Reference to Door Pivot")]
    public DoorController door;  // Drag DoorPivot here

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered door close trigger.");

            if (door != null)
            {
                door.CloseDoor(); // Rotate to 90°
                Debug.Log("CloseDoor() called on: " + door.name);
            }
            else
            {
                Debug.LogWarning("Door reference not assigned in DoorCloseTrigger!");
            }
        }
    }
}
