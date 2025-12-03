using UnityEngine;

public class FlowerMachine : MonoBehaviour, InteractableScript
{
    [Header("Flower Requirements")]
    [SerializeField] private bool sonFlower = false;
    [SerializeField] private bool daughterFlower = false;
    [SerializeField] private bool momFlower = false;

    [Header("Generator Requirements")]
    [SerializeField] private bool generator1On = false;
    [SerializeField] private bool generator2On = false;

    [Header("Lights (Turn Red When Power Off)")]
    [SerializeField] private Light[] lightsToTurnRed;
    [SerializeField] private Color originalLightColor = Color.white;
    [SerializeField] private Color powerOffColor = Color.red;
    [SerializeField] private float powerOffIntensity = 2f;

    [Header("Machine State")]
    [SerializeField] private bool hasShutdownPower = false; // Track if power has been shut down
    [SerializeField] private bool machineCompleted = false;

    [Header("Optional Dialogue")]
    [SerializeField] private DialogueTrigger noFlowersDialogue; // "Need all 3 flowers"
    [SerializeField] private DialogueTrigger powerShutdownDialogue; // "Power is shutting down!"
    [SerializeField] private DialogueTrigger needPowerDialogue; // "Need to restore power"
    [SerializeField] private DialogueTrigger machineStartDialogue; // "Machine is starting..."

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private float[] originalIntensities;

    private void Start()
    {
        // Store original light intensities
        if (lightsToTurnRed != null && lightsToTurnRed.Length > 0)
        {
            originalIntensities = new float[lightsToTurnRed.Length];
            for (int i = 0; i < lightsToTurnRed.Length; i++)
            {
                if (lightsToTurnRed[i] != null)
                {
                    originalIntensities[i] = lightsToTurnRed[i].intensity;
                }
            }
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

        // You could add screen shake, sound effects, etc. here
    }

    private void TurnLightsRed()
    {
        if (lightsToTurnRed == null || lightsToTurnRed.Length == 0)
            return;

        for (int i = 0; i < lightsToTurnRed.Length; i++)
        {
            if (lightsToTurnRed[i] != null)
            {
                lightsToTurnRed[i].color = powerOffColor;
                lightsToTurnRed[i].intensity = powerOffIntensity;
            }
        }

        if (showDebugLogs)
            Debug.Log($"Turned {lightsToTurnRed.Length} lights red!");
    }

    private void RestoreLights()
    {
        if (lightsToTurnRed == null || lightsToTurnRed.Length == 0)
            return;

        for (int i = 0; i < lightsToTurnRed.Length; i++)
        {
            if (lightsToTurnRed[i] != null && originalIntensities != null)
            {
                lightsToTurnRed[i].color = originalLightColor;
                lightsToTurnRed[i].intensity = originalIntensities[i];
            }
        }

        if (showDebugLogs)
            Debug.Log("Lights restored to original color!");
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

        // ===== ADD YOUR MACHINE COMPLETION LOGIC HERE =====
        // Examples:
        // - Play animation
        // - Spawn object
        // - Open door
        // - Trigger cutscene
        // - Update progress manager
        // - etc.

        OnMachineComplete();
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

    private void CheckGeneratorStatus()
    {
        if (BothGeneratorsOn() && hasShutdownPower)
        {
            if (showDebugLogs)
                Debug.Log("Both generators online! Power restored. Return to the machine!");

            // Optionally restore lights immediately when generators come on
            // RestoreLights();
        }
    }

    // ===== EXTEND THIS METHOD FOR YOUR GAME LOGIC =====
    private void OnMachineComplete()
    {
        // TODO: Add what happens when machine completes
        // Examples:

        // Open a door
        // Door door = FindObjectOfType<Door>();
        // if (door != null) door.Open();

        // Update progress manager
        // if (ProgressManager.Instance != null)
        //     ProgressManager.Instance.machineCompleted = true;

        // Spawn an object
        // if (rewardPrefab != null)
        //     Instantiate(rewardPrefab, spawnPoint.position, spawnPoint.rotation);

        // Play a cutscene
        // CutsceneManager.Instance.PlayCutscene("MachineComplete");

        if (showDebugLogs)
            Debug.Log("Machine completion logic goes here!");
    }

    // ===== DEBUG HELPERS (Remove in final build) =====

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