using UnityEngine;

public class SyringePickUp : MonoBehaviour, InteractableScript, stage2_InteractableScript
{
    [SerializeField] public Inventory inventory;

    void Start()
    {
        if (inventory == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                inventory = player.GetComponent<Inventory>();
        }
    }

    public void InteractScript()
    {
        if (inventory == null) return;

        if (!inventory.holdingSyringe)
        {
            inventory.pickUpSyringe();
            Destroy(gameObject);
        }
    }
}