using UnityEngine;

public class Generator : MonoBehaviour, InteractableScript
{
    public enum GeneratorNumber { Generator1, Generator2 }

    [SerializeField] private GeneratorNumber generatorNumber;
    [SerializeField] private FlowerMachine machine;
    [SerializeField] private bool isOn = false;

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

        switch (generatorNumber)
        {
            case GeneratorNumber.Generator1:
                machine.SetGenerator1On();
                break;
            case GeneratorNumber.Generator2:
                machine.SetGenerator2On();
                break;
        }

        // TODO: Add visual feedback (lights, particles, etc.)
        Debug.Log($"{generatorNumber} turned ON!");
    }
}