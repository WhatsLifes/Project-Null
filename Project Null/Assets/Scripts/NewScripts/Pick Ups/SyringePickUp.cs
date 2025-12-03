using UnityEngine;

public class SyringePickUp : MonoBehaviour, InteractableScript, stage2_InteractableScript
{
    [SerializeField] public Inventory inventory;
    public void InteractScript()
    {
        if(!inventory.holdingSyringe)
        {
            inventory.pickUpSyringe();
            Destroy(gameObject);
        }
    }
}
