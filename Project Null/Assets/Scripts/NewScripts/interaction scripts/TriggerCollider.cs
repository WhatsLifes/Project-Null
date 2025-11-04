using UnityEngine;

public class TriggerCollider : MonoBehaviour
{
    [SerializeField] private InteractableObject interactableObject;
    
    // when you enter the collider
    private void OnTriggerEnter(Collider other)
    {
        // check for player
        if (other.CompareTag("Player"))
        {
            interactableObject.SetPlayerNearby(true);  // set us nearby
            interactableObject.ShowWhiteDot();   // show the white dot
            PlayerInteraction.instance.AddNearbyObject(interactableObject);  // adds the object to nearby list
        }
    }

    // when you leave the collider
    private void OnTriggerExit(Collider other)
    {
        // check for player
        if (other.CompareTag("Player"))
        {
            interactableObject.SetPlayerNearby(false); // set us *not* nearby
            interactableObject.HideWhiteDot();  // hide the white dot
            interactableObject.HidePrompt();  // make sure we dont see the prompt
            PlayerInteraction.instance.RemoveNearbyObject(interactableObject);  // remove the object from nearby list
        }
    }
    
}

