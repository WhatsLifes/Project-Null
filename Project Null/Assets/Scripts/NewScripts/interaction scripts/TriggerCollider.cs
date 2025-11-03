using UnityEngine;

public class TriggerCollider : MonoBehaviour
{
    [SerializeField] private InteractableObject interactableObject;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactableObject.SetPlayerNearby(true);
            interactableObject.ShowWhiteDot();
            PlayerInteraction.instance.AddNearbyObject(interactableObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactableObject.SetPlayerNearby(false);
            interactableObject.HideWhiteDot();
            interactableObject.HidePrompt();
            PlayerInteraction.instance.RemoveNearbyObject(interactableObject);
        }
    }
    
}

