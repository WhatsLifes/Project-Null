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
    public GameObject[] mannequinsToAppear; // Drag mannequins that will appear after lights go out
    public float mannequinDestroyDelay = 0.2f; // Time after lights turn off before mannequins disappear

    [Header("Interaction Settings")]
    public float interactDistance = 3f; // Player must be this close
    public KeyCode interactKey = KeyCode.E;

    [Header("Delay Settings")]
    public float openDelay = 1.5f; // Time in seconds before the doors open

    [Header("Audio Settings")]
    public AudioSource audioSource;      // Optional: drag an AudioSource here or leave blank to auto-create
    public AudioClip buttonPressSound;   // 🎵 Drag your .mp3, .wav, or .ogg file here
    [Range(0f, 1f)] public float volume = 1f;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Auto-setup audio if not already assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        // Hide all mannequins that will appear later
        HideMannequinsToAppear();
    }

    public void InteractScript()
    {
        if (player == null || mannequinObsDoor == null) return;

        // 🔊 Play button press sound
        if (buttonPressSound != null)
        {
            audioSource.PlayOneShot(buttonPressSound, volume);
        }

        // Start the delayed opening
        StartCoroutine(OpenDoorsWithDelay());
    }

    private IEnumerator OpenDoorsWithDelay()
    {
        Debug.Log("Button pressed - waiting before opening doors...");

        // Turn off all lights
        TurnOffLights();

        // Wait a brief moment in darkness, then swap mannequins
        yield return new WaitForSeconds(mannequinDestroyDelay);
        DestroyMannequins();
        ShowMannequinsToAppear(); // Make new mannequins appear
        mannequinDoor.OpenDoor();

        // Continue with the rest of the delay
        yield return new WaitForSeconds(openDelay - mannequinDestroyDelay);

        // Open both doors
        mannequinObsDoor.OpenDoor();
        Debug.Log("Doors are now opening!");

        GameProgressManager.Instance.buttonPressed = true;
        Debug.Log("Progress updated: buttonPressed = true");

        // Optional: wait for doors to finish opening
        float doorOpenDuration = 0f; // adjust if needed
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
                light.enabled = false;
        }
        Debug.Log($"Turned off {lightsToTurnOff.Length} lights");
    }

    private void TurnOnLights()
    {
        if (lightsToTurnOff == null || lightsToTurnOff.Length == 0) return;

        foreach (Light light in lightsToTurnOff)
        {
            if (light != null)
                light.enabled = true;
        }
    }

    private void DestroyMannequins()
    {
        if (mannequinsToDestroy == null || mannequinsToDestroy.Length == 0) return;

        foreach (GameObject mannequin in mannequinsToDestroy)
        {
            if (mannequin != null)
                Destroy(mannequin);
        }
        Debug.Log($"Destroyed {mannequinsToDestroy.Length} mannequins in the darkness!");
    }

    private void HideMannequinsToAppear()
    {
        if (mannequinsToAppear == null || mannequinsToAppear.Length == 0) return;

        foreach (GameObject mannequin in mannequinsToAppear)
        {
            if (mannequin != null)
                mannequin.SetActive(false);
        }
        Debug.Log($"Hid {mannequinsToAppear.Length} mannequins at start");
    }

    private void ShowMannequinsToAppear()
    {
        if (mannequinsToAppear == null || mannequinsToAppear.Length == 0) return;

        foreach (GameObject mannequin in mannequinsToAppear)
        {
            if (mannequin != null)
                mannequin.SetActive(true);
        }
        Debug.Log($"Showed {mannequinsToAppear.Length} new mannequins - they moved in the darkness!");
    }
}
