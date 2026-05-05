using UnityEngine;

public class FlowerPlacement : MonoBehaviour, InteractableScript
{
    public enum FlowerType { Son, Daughter, Mom }

    [Header("Placement Settings")]
    [SerializeField] private FlowerType flowerType;
    [SerializeField] private FlowerMachine machine;

    [Header("Visual Flower (Pre-placed in container)")]
    [SerializeField] private GameObject flowerVisual; // The actual flower mesh in the container

    [Header("Pickup Settings")]
    [Tooltip("Does this flower need to be picked up first? (Uncheck for Son/Daughter, check for Mom)")]
    [SerializeField] private bool requiresPickup = false;

    [Tooltip("Has the flower been picked up? (Set automatically or check for bypass)")]
    public bool flowerPickedUp = false;

    [Header("State")]
    [SerializeField] private bool isPlaced = false;

    [Header("Optional Dialogue")]
    [SerializeField] private DialogueTrigger needFlowerDialogue; // "I need to find the flower first"
    [SerializeField] private DialogueTrigger placedDialogue; // "Flower placed!"

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    [SerializeField] private HUD hud;


    private void Start()
    {
        // Hide flower visual at start
        if (flowerVisual != null)
            flowerVisual.SetActive(false);
    }

    public void InteractScript()
    {
        if (isPlaced)
        {
            if (showDebugLogs)
                Debug.Log($"{flowerType} flower already placed!");
            return;
        }

        // Check if flower needs to be picked up first
        if (requiresPickup && !flowerPickedUp)
        {
            if (showDebugLogs)
                Debug.Log($"Need to find {flowerType} flower first!");

            if (needFlowerDialogue != null)
                needFlowerDialogue.TriggerNow();

            return;
        }

        // Place the flower
        PlaceFlower();
    }

    private void PlaceFlower()
    {
        isPlaced = true;

        // Show the flower visual
        if (flowerVisual != null)
            flowerVisual.SetActive(true);

        // Notify machine
        if (machine != null)
        {
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
        }
        else
        {
            Debug.LogError("FlowerMachine not assigned!");
        }

        if (showDebugLogs)
            Debug.Log($"✓ {flowerType} flower placed!");

        if (placedDialogue != null)
            placedDialogue.TriggerNow();
    }

    // Called by FlowerPickup when mom flower is picked up
    public void MarkFlowerAsPickedUp()
    {
        flowerPickedUp = true;
        if (flowerType == FlowerType.Mom && hud != null)
            hud.FoundLastFlower();
        if (showDebugLogs) Debug.Log($"{flowerType} flower marked as picked up!");
    }

    [ContextMenu("Debug: Reset Placement")]
    private void DebugResetPlacement()
    {
        isPlaced = false;
        flowerPickedUp = false;
        if (flowerVisual != null)
            flowerVisual.SetActive(false);
        if (showDebugLogs)
            Debug.Log($"DEBUG: {flowerType} placement reset!");
    }

    [ContextMenu("Debug: Mark As Picked Up")]
    private void DebugMarkPickedUp()
    {
        flowerPickedUp = true;
        if (showDebugLogs)
            Debug.Log($"DEBUG: {flowerType} marked as picked up!");
    }
}