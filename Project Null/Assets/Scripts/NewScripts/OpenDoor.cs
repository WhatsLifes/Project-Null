using UnityEngine;
using System.Collections; // Needed for IEnumerator and WaitForSeconds

public class DoorButton : MonoBehaviour, InteractableScript
{
    [Header("Reference to Door Pivot")]
    public DoorController door; // Drag DoorPivot here
    public DoorController door2;

    [Header("Interaction Settings")]
    public float interactDistance = 3f; // Player must be this close
    public KeyCode interactKey = KeyCode.E;

    [Header("Delay Settings")]
    public float openDelay = 1.5f; // Time in seconds before the doors open

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public void InteractScript()
    {
        if (player == null || door == null) return;

        // Start the delayed opening
        StartCoroutine(OpenDoorsWithDelay());
    }

    private IEnumerator OpenDoorsWithDelay()
    {
        Debug.Log("Button pressed - waiting before opening doors...");
        yield return new WaitForSeconds(openDelay); // Wait the set amount of time

        // Open both doors
        door.OpenDoor();
        door2.OpenDoor();
        Debug.Log("Doors are now opening!");

        GameProgressManager.Instance.buttonPressed = true;
        Debug.Log("Progress updated: buttonPressed = true");
    }
}
