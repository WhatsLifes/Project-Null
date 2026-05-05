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
            {
                inventory = player.GetComponent<Inventory>();
            }
            else
            {
                Debug.LogError("Player not found for SyringePickUp");
            }
        }
    }

    public void InteractScript()
    {
        Debug.Log("Trying to pick up syringe");

        if (!Inventory.holdingSyringe)
        {
            Debug.Log("Picked up syringe");

            Inventory.holdingSyringe = true;
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Already holding syringe");
        }
    }
}