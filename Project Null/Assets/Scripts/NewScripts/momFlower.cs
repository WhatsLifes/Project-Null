using UnityEngine;

public class FlowerPickup : MonoBehaviour, InteractableScript
{
    [SerializeField] private FlowerPlacement momFlowerPlacement;
    [SerializeField] private HUD hud;


    public void InteractScript()
    {
        if (momFlowerPlacement == null)
        {
            Debug.LogError("Mom Flower Placement not assigned!");
            return;
        }

        // Mark mom flower as picked up
        momFlowerPlacement.MarkFlowerAsPickedUp();

        Debug.Log("Picked up Mom's flower!");

        // Hide the flower pickup
        gameObject.SetActive(false);
    }
}