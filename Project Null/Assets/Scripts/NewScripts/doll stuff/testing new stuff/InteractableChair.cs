using UnityEngine;

public class InteractableChair : MonoBehaviour, InteractableScript
{
    private ChairSlot chairSlot;

    void Awake()
    {
        chairSlot = GetComponent<ChairSlot>();
    }

    public bool IsSlotFilled()
    {
        if (chairSlot == null) return true; 
        return chairSlot.HasDoll();
    }

    public void InteractScript()
    {
        if (PlayerHold.Instance != null && PlayerHold.Instance.IsHoldingObject() && !chairSlot.HasDoll())
        {
            GameObject dollToPlace = PlayerHold.Instance.Place();
            
            if (dollToPlace != null)
            {
                chairSlot.PlaceDoll(dollToPlace);
            }
        }
    }
}