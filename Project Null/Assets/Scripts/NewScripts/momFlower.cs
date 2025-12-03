using UnityEngine;

public class FlowerPickup : MonoBehaviour, InteractableScript
{
    public enum FlowerType { Son, Daughter, Mom }

    [SerializeField] private FlowerType flowerType;
    [SerializeField] private FlowerMachine machine;

    public void InteractScript()
    {
        if (machine == null)
        {
            Debug.LogError("Machine not assigned!");
            return;
        }

        switch (flowerType)
        {
            case FlowerType.Son:
                machine.PlaceSonFlower();
                break;
            case FlowerType.Daughter:
                machine.PlaceDaughterFlower();
                break;
            case FlowerType.Mom:
                machine.PlaceMomFlower();
                break;
        }

        // Hide the flower
        gameObject.SetActive(false);
    }
}