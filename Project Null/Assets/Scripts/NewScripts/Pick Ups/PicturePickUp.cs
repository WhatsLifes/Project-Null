using UnityEngine;

public class PicturePickUp : MonoBehaviour, InteractableScript
{
    [SerializeField] public Inventory inventory;
    public void InteractScript()
    {
        inventory.pickUpPicture();
        Destroy(gameObject);
    }
}