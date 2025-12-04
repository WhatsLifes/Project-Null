using UnityEngine;

public class Generator : MonoBehaviour, InteractableScript
{
    public enum GeneratorNumber { Generator1, Generator2 }

    [SerializeField] private GeneratorNumber generatorNumber;
    [SerializeField] private FlowerMachine machine;
    [SerializeField] private bool isOn = false;

    [Header("Visual Feedback")]
    [SerializeField] private Light generatorLight; // The light that should turn green
    [SerializeField] private Color onColor = Color.green;
    [SerializeField] private float onIntensity = 5f;
    [SerializeField] private GameObject onVisual; // Optional: particles, etc.
    [SerializeField] private GameObject offVisual; // Optional: smoke, sparks, etc.

    private Color originalLightColor;
    private float originalLightIntensity;

    private void Start()
    {
        // Store original light settings
        if (generatorLight != null)
        {
            originalLightColor = generatorLight.color;
            originalLightIntensity = generatorLight.intensity;
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

        // Turn on the generator in the machine
        switch (generatorNumber)
        {
            case GeneratorNumber.Generator1:
                machine.SetGenerator1On();
                break;
            case GeneratorNumber.Generator2:
                machine.SetGenerator2On();
                break;
        }

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

        Debug.Log($"{generatorNumber} turned ON!");
    }
}