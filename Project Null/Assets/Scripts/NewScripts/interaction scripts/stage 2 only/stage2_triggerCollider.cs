using UnityEngine;

public class stage2_triggerCollider : MonoBehaviour
{
    [SerializeField] private stage2_interactableOBJ interactableObject;
    
    // when you enter the collider
    private void OnTriggerEnter(Collider other)
    {
        // check for player
        if (other.CompareTag("Player"))
        {
            interactableObject.SetPlayerNearby(true);  // set us nearby
            interactableObject.ShowWhiteDot();   // show the white dot
            stage2_interaction.instance.AddNearbyObject(interactableObject);  // adds the object to nearby list
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
            stage2_interaction.instance.RemoveNearbyObject(interactableObject);  // remove the object from nearby list
        }
    }
    
}