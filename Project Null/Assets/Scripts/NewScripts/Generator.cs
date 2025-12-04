using UnityEngine;

public class Generator : MonoBehaviour, InteractableScript
{
    public enum GeneratorNumber { Generator1, Generator2 }

    [SerializeField] private GeneratorNumber generatorNumber;
    [SerializeField] private FlowerMachine machine;
    [SerializeField] private bool isOn = false;

    [Header("Visual Feedback")]
    [SerializeField] private Light generatorLight;
    [SerializeField] private Color onColor = Color.green;
    [SerializeField] private float onIntensity = 5f;
    [SerializeField] private GameObject onVisual;
    [SerializeField] private GameObject offVisual;

    [Header("HUD Settings")]
    [SerializeField] private HUD hud;

    private Color originalLightColor;
    private float originalLightIntensity;
    private bool wasOn = false;

    private void Start()
    {
        // Store original light settings
        if (generatorLight != null)
        {
            originalLightColor = generatorLight.color;
            originalLightIntensity = generatorLight.intensity;
        }

        // Auto-find HUD if not assigned
        if (hud == null)
        {
            hud = FindObjectOfType<HUD>();
        }

        wasOn = isOn;

        // Apply initial state
        if (isOn)
        {
            TurnOnVisuals();
        }
    }

    private void Update()
    {
        // Check if isOn was toggled in Inspector during runtime
        if (isOn != wasOn)
        {
            wasOn = isOn;

            if (isOn)
            {
                TurnOnGenerator();
            }
        }
    }

    public void InteractScript()
    {
        if (isOn)
        {
            Debug.Log("Generator already on!");
            return;
        }

        if (machine == null)
        {
            Debug.LogError("Machine not assigned!");
            return;
        }

        isOn = true;
        TurnOnGenerator();
    }

    private void TurnOnGenerator()
    {
        // Notify the machine
        if (machine != null)
        {
            switch (generatorNumber)
            {
                case GeneratorNumber.Generator1:
                    machine.SetGenerator1On();
                    break;
                case GeneratorNumber.Generator2:
                    machine.SetGenerator2On();
                    break;
            }

            // Update objective based on how many generators are on
            UpdateObjective();
        }

        // Turn on visuals
        TurnOnVisuals();

        Debug.Log($"{generatorNumber} turned ON!");
    }

    private void UpdateObjective()
    {
        if (hud == null || machine == null) return;

        // Check how many generators are on
        int generatorsOn = 0;
        if (machine.IsGenerator1On()) generatorsOn++;
        if (machine.IsGenerator2On()) generatorsOn++;

        // Update objective based on count
        if (generatorsOn == 1)
        {
            hud.ShowObjective16(); // "Turn on the generators (1/2)"
            Debug.Log("First generator on - Objective 16 shown");
        }
        else if (generatorsOn == 2)
        {
            hud.ShowObjective17(); // "Return to the workstation & try again"
            Debug.Log("Both generators on - Objective 17 shown");
        }
    }

    private void TurnOnVisuals()
    {
        // Turn this generator's light green
        if (generatorLight != null)
        {
            generatorLight.color = onColor;
            generatorLight.intensity = onIntensity;
        }

        // Visual feedback
        if (onVisual != null)
            onVisual.SetActive(true);

        if (offVisual != null)
            offVisual.SetActive(false);
    }
}