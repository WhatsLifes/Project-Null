using UnityEngine;

public class PickupFlower : MonoBehaviour, stage2_InteractableScript
{
    public enum FlowerType
    {
        BoyFlower,
        GirlFlower
    }

    [Header("Flower Settings")]
    public FlowerType flowerType;

    [Header("Light Controller")]
    [SerializeField] private LightTurnOnAndOff lightController;

    [Header("Optional Dialogue")]
    public DialogueTrigger dialogueTrigger;

    public void InteractScript()
    {
        Debug.Log("[PickupFlower] InteractScript called for: " + flowerType.ToString());

        // Update progress manager
        if (flowerType == FlowerType.BoyFlower)
        {
            Stage2_5ProgressManager.Instance.boyFlowerPickedUp = true;
            Debug.Log("[PickupFlower] Boy flower picked up.");
        }
        else if (flowerType == FlowerType.GirlFlower)
        {
            Stage2_5ProgressManager.Instance.girlFlowerPickedUp = true;
            Debug.Log("[PickupFlower] Girl flower picked up.");
        }

        // Trigger dialogue if present
        if (dialogueTrigger != null)
            dialogueTrigger.TriggerNow();

        // Switch lights
        if (lightController != null)
            lightController.SwitchLights();

        // Hide visual + collider
        HideFlower();
    }

    private void HideFlower()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Debug.Log("[PickupFlower] Flower hidden.");
    }
}
