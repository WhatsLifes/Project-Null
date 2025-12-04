using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class FlowerMachine : MonoBehaviour, InteractableScript
{
    [Header("Flower Requirements")]
    [SerializeField] private bool sonFlower = false;
    [SerializeField] private bool daughterFlower = false;
    [SerializeField] private bool momFlower = false;

    [Header("Generator Requirements")]
    [SerializeField] private bool generator1On = false;
    [SerializeField] private bool generator2On = false;

    [Header("Amp Post Lights (Amber → Red)")]
    [SerializeField] private Light[] ampPostLights;
    [SerializeField] private Color ampPostOriginalColor = new Color(1f, 0.5f, 0f); // Amber/Orange
    [SerializeField] private Color ampPostPowerOffColor = Color.red;
    [SerializeField] private float ampPostPowerOffIntensity = 2f;

    [Header("Generator Lights (Green → Red)")]
    [SerializeField] private Light[] generatorLights;
    [SerializeField] private Color generatorOriginalColor = Color.green;
    [SerializeField] private Color generatorPowerOffColor = Color.red;
    [SerializeField] private float generatorPowerOffIntensity = 2f;

    [Header("Machine State")]
    [SerializeField] private bool hasShutdownPower = false;
    [SerializeField] private bool machineCompleted = false;

    [Header("Optional Dialogue")]
    [SerializeField] private DialogueTrigger noFlowersDialogue;
    [SerializeField] private DialogueTrigger powerShutdownDialogue;
    [SerializeField] private DialogueTrigger needPowerDialogue;
    [SerializeField] private DialogueTrigger machineStartDialogue;

    [Header("HUD")]
    [SerializeField] private HUD hud;

    [Header("Win Condition (Ending)")]
    [SerializeField] private EndingManager endingManager;
    [SerializeField] private AudioSource machineAudioSource;
    [SerializeField] private AudioClip machineSuccessSound;
    [SerializeField] private float delayBeforeEnding = 5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private float[] ampPostOriginalIntensities;
    private float[] generatorOriginalIntensities;

    private void Start()
    {
        // Store original intensities for amp post lights
        if (ampPostLights != null && ampPostLights.Length > 0)
        {
            ampPostOriginalIntensities = new float[ampPostLights.Length];
            for (int i = 0; i < ampPostLights.Length; i++)
            {
                if (ampPostLights[i] != null)
                {
                    ampPostOriginalIntensities[i] = ampPostLights[i].intensity;
                }
            }
        }

        // Store original intensities for generator lights
        if (generatorLights != null && generatorLights.Length > 0)
        {
            generatorOriginalIntensities = new float[generatorLights.Length];
            for (int i = 0; i < generatorLights.Length; i++)
            {
                if (generatorLights[i] != null)
                {
                    generatorOriginalIntensities[i] = generatorLights[i].intensity;
                }
            }
        }

        // Auto-find HUD
        if (hud == null)
        {
            hud = FindObjectOfType<HUD>();
        }

        // Auto-find ending manager
        if (endingManager == null)
        {
            endingManager = FindObjectOfType<EndingManager>();
        }
    }

    public void InteractScript()
    {
        if (machineCompleted)
        {
            if (showDebugLogs)
                Debug.Log("Machine already completed!");
            return;
        }

        // Check if all flowers are placed
        if (!AllFlowersPlaced())
        {
            if (showDebugLogs)
                Debug.Log("Not all flowers placed! Need all 3 flowers.");

            if (noFlowersDialogue != null)
                noFlowersDialogue.TriggerNow();

            return;
        }

        // All flowers are placed - now check power state
        if (!hasShutdownPower)
        {
            // First activation - shut down power
            ShutdownPower();
        }
        else if (!BothGeneratorsOn())
        {
            // Power is still off, generators not restored
            if (showDebugLogs)
                Debug.Log("Power is off! Need to restore both generators.");

            if (needPowerDialogue != null)
                needPowerDialogue.TriggerNow();
        }
        else
        {
            // All conditions met - activate machine!
            ActivateMachine();
        }
    }

    private bool AllFlowersPlaced()
    {
        return sonFlower && daughterFlower && momFlower;
    }

    private bool BothGeneratorsOn()
    {
        return generator1On && generator2On;
    }

    private void ShutdownPower()
    {
        if (showDebugLogs)
            Debug.Log("POWER SHUTTING DOWN! All flowers placed, but generators are failing!");

        hasShutdownPower = true;

        // Turn all lights red
        TurnLightsRed();

        // Play shutdown dialogue
        if (powerShutdownDialogue != null)
            powerShutdownDialogue.TriggerNow();

        // Show objective to turn on generators
        if (hud != null)
        {
            hud.ShowObjective15(); // "Turn on the generators (0/2)"
            Debug.Log("Power shutdown - Objective 15 shown");
        }
    }

    private void TurnLightsRed()
    {
        // Turn amp post lights red
        if (ampPostLights != null)
        {
            for (int i = 0; i < ampPostLights.Length; i++)
            {
                if (ampPostLights[i] != null)
                {
                    ampPostLights[i].color = ampPostPowerOffColor;
                    ampPostLights[i].intensity = ampPostPowerOffIntensity;
                }
            }
        }

        // Turn generator lights red
        if (generatorLights != null)
        {
            for (int i = 0; i < generatorLights.Length; i++)
            {
                if (generatorLights[i] != null)
                {
                    generatorLights[i].color = generatorPowerOffColor;
                    generatorLights[i].intensity = generatorPowerOffIntensity;
                }
            }
        }

        if (showDebugLogs)
            Debug.Log($"Turned {(ampPostLights?.Length ?? 0)} amp post lights and {(generatorLights?.Length ?? 0)} generator lights red!");
    }

    private void RestoreAmpPostLights()
    {
        // Restore amp post lights to amber
        if (ampPostLights != null)
        {
            for (int i = 0; i < ampPostLights.Length; i++)
            {
                if (ampPostLights[i] != null && ampPostOriginalIntensities != null)
                {
                    ampPostLights[i].color = ampPostOriginalColor;
                    ampPostLights[i].intensity = ampPostOriginalIntensities[i];
                }
            }
        }

        if (showDebugLogs)
            Debug.Log("Amp post lights restored to amber!");
    }

    private void RestoreLights()
    {
        // Restore amp post lights to amber
        RestoreAmpPostLights();

        // Restore generator lights to green
        if (generatorLights != null)
        {
            for (int i = 0; i < generatorLights.Length; i++)
            {
                if (generatorLights[i] != null && generatorOriginalIntensities != null)
                {
                    generatorLights[i].color = generatorOriginalColor;
                    generatorLights[i].intensity = generatorOriginalIntensities[i];
                }
            }
        }

        if (showDebugLogs)
            Debug.Log("All lights restored to original colors!");
    }

    private void ActivateMachine()
    {
        if (showDebugLogs)
            Debug.Log("★★★ MACHINE ACTIVATING! ★★★");

        machineCompleted = true;

        // Restore lights to normal
        RestoreLights();

        // Play machine start dialogue
        if (machineStartDialogue != null)
            machineStartDialogue.TriggerNow();

        // Start the ending sequence
        StartCoroutine(MachineSuccessSequence());
    }

    private IEnumerator MachineSuccessSequence()
    {
        // Play success sound
        if (machineAudioSource != null && machineSuccessSound != null)
        {
            machineAudioSource.PlayOneShot(machineSuccessSound);
            if (showDebugLogs)
                Debug.Log("Playing machine success sound...");
        }

        // Wait for the delay
        if (showDebugLogs)
            Debug.Log($"Waiting {delayBeforeEnding} seconds before triggering ending...");

        yield return new WaitForSeconds(delayBeforeEnding);

        // Trigger ending instead of death
        if (endingManager != null)
        {
            if (showDebugLogs)
                Debug.Log("🎬 TRIGGERING ENDING SEQUENCE!");

            endingManager.TriggerEnding();
        }
        else
        {
            Debug.LogError("EndingManager not found! Cannot trigger ending.");
        }
    }

    // ===== PUBLIC METHODS TO CALL FROM OTHER SCRIPTS =====

    public void PlaceSonFlower()
    {
        sonFlower = true;
        if (showDebugLogs)
            Debug.Log("Son's flower placed!");
    }

    public void PlaceDaughterFlower()
    {
        daughterFlower = true;
        if (showDebugLogs)
            Debug.Log("Daughter's flower placed!");
    }

    public void PlaceMomFlower()
    {
        momFlower = true;
        if (showDebugLogs)
            Debug.Log("Mom's flower placed!");
    }

    public void SetGenerator1On()
    {
        generator1On = true;
        if (showDebugLogs)
            Debug.Log("Generator 1 turned on!");

        CheckGeneratorStatus();
    }

    public void SetGenerator2On()
    {
        generator2On = true;
        if (showDebugLogs)
            Debug.Log("Generator 2 turned on!");

        CheckGeneratorStatus();
    }

    public bool IsGenerator1On()
    {
        return generator1On;
    }

    public bool IsGenerator2On()
    {
        return generator2On;
    }

    private void CheckGeneratorStatus()
    {
        if (BothGeneratorsOn() && hasShutdownPower)
        {
            // Restore amp post lights when both generators are on
            RestoreAmpPostLights();

            if (showDebugLogs)
                Debug.Log("Both generators online! Power restored. Amp post lights restored to amber!");
        }
    }

    // ===== DEBUG HELPERS =====

    [ContextMenu("Debug: Place All Flowers")]
    private void DebugPlaceAllFlowers()
    {
        sonFlower = true;
        daughterFlower = true;
        momFlower = true;
        Debug.Log("DEBUG: All flowers placed!");
    }

    [ContextMenu("Debug: Turn On All Generators")]
    private void DebugTurnOnGenerators()
    {
        generator1On = true;
        generator2On = true;
        CheckGeneratorStatus();
        Debug.Log("DEBUG: All generators on!");
    }

    [ContextMenu("Debug: Reset Machine")]
    private void DebugResetMachine()
    {
        sonFlower = false;
        daughterFlower = false;
        momFlower = false;
        generator1On = false;
        generator2On = false;
        hasShutdownPower = false;
        machineCompleted = false;
        RestoreLights();
        Debug.Log("DEBUG: Machine reset!");
    }
}