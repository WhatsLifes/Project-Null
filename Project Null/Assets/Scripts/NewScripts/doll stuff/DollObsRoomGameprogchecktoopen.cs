using UnityEngine;

public class DollObsRoomDoor : MonoBehaviour
{
    [Header("Reference to Door Pivot")]
    public DoorController door; // Assign the door pivot that opens/closes

    private bool doorOpened = false;

    void Update()
    {
        if (doorOpened || GameProgressManager.Instance == null || door == null)
            return;

        // Check if both conditions from the progress manager are true
        if (GameProgressManager.Instance.buttonPressed && GameProgressManager.Instance.puzzleCompleted)
        {
            door.OpenDoor(); // Call your existing door open method
            doorOpened = true; // Prevent reopening spam
            Debug.Log("✅ Both conditions met — DollObsRoomDoor opening!");
        }
    }
}
