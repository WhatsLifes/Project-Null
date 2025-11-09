using UnityEngine;
using System.Collections; // Needed for IEnumerator and WaitForSeconds

public class DoorButton : MonoBehaviour, InteractableScript
{
    [Header("Reference to Door Pivot")]
    public DoorController mannequinObsDoor; // Drag DoorPivot here
    public DoorController mannequinDoor;

    [Header("Light Settings")]
    public Light[] lightsToTurnOff; // Drag all lights you want to turn off here

    [Header("Mannequin Settings")]
    public GameObject[] mannequinsToDestroy; // Drag mannequin GameObjects here
    public float mannequinDestroyDelay = 0.2f; // Time after lights turn off before mannequins disappear

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
        if (player == null || mannequinObsDoor == null) return;

        // Start the delayed opening
        StartCoroutine(OpenDoorsWithDelay());
    }

    private IEnumerator OpenDoorsWithDelay()
    {
        Debug.Log("Button pressed - waiting before opening doors...");

        // Turn off all lights
        TurnOffLights();

        // Wait a brief moment in darkness, then destroy mannequins
        yield return new WaitForSeconds(mannequinDestroyDelay);
        DestroyMannequins();
        mannequinDoor.OpenDoor();

        // Continue with the rest of the delay
        yield return new WaitForSeconds(openDelay - mannequinDestroyDelay);

        // Open both doors
        mannequinObsDoor.OpenDoor();
        Debug.Log("Doors are now opening!");

        GameProgressManager.Instance.buttonPressed = true;
        Debug.Log("Progress updated: buttonPressed = true");

        // Wait for doors to finish opening (get the duration from door controller if available)
        // Assuming doors take some time to open - adjust this value based on your door animation
        float doorOpenDuration = 0f; // Change this to match your door's opening time
        yield return new WaitForSeconds(doorOpenDuration);

        // Turn lights back on
        TurnOnLights();
        Debug.Log("Lights turned back on!");
    }

    private void TurnOffLights()
    {
        if (lightsToTurnOff == null || lightsToTurnOff.Length == 0) return;

        foreach (Light light in lightsToTurnOff)
        {
            if (light != null)
            {
                light.enabled = false;
            }
        }
        Debug.Log($"Turned off {lightsToTurnOff.Length} lights");
    }

    private void TurnOnLights()
    {
        if (lightsToTurnOff == null || lightsToTurnOff.Length == 0) return;

        foreach (Light light in lightsToTurnOff)
        {
            if (light != null)
            {
                light.enabled = true;
            }
        }
    }

    private void DestroyMannequins()
    {
        if (mannequinsToDestroy == null || mannequinsToDestroy.Length == 0) return;

        foreach (GameObject mannequin in mannequinsToDestroy)
        {
            if (mannequin != null)
            {
                Destroy(mannequin);
            }
        }
        Debug.Log($"Destroyed {mannequinsToDestroy.Length} mannequins in the darkness!");
    }
}