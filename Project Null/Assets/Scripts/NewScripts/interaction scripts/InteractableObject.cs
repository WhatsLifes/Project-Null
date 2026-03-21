using TMPro;
using UnityEngine;

// think of this like a struct, but anything with this must implement whats in here
interface InteractableScript
{
   public void InteractScript();
}

public class InteractableObject : MonoBehaviour
{
   [Header("UI Elements")]
   [SerializeField] private Canvas whiteDotCanvas;
   [SerializeField] private Canvas promptCanvas;

   [Header("Audio Elements")]
   [SerializeField] private AudioSource audioSource;
   [SerializeField] private AudioClip interactSound;

   private bool isPLayerNearby = false;

   public bool IsPLayerNearby => isPLayerNearby;

   // turns off the prompts 
   private void Start()
   {
      SetCanvasState(whiteDotCanvas, false);
      SetCanvasState(promptCanvas, false);
   }

   public virtual void InteractItem()
   {
      // gets the object we are trying to interact with
      InteractableScript TheInteractScript = gameObject.GetComponent<InteractableScript>();
      PlayerInteraction.instance.RemoveNearbyObject(this);  // take the object out of the nearby interact list
      HidePrompt();  // hide prompt
      HideWhiteDot();  // hide dot

      // call the interact script 
      TheInteractScript.InteractScript();

      // play interaction sound
      audioSource.PlayOneShot(interactSound);
   }

   public void ShowPrompt()
   {
      HideWhiteDot();
      SetCanvasState(promptCanvas, true);
   }

   public void HidePrompt()
   {
      SetCanvasState(promptCanvas, false);
   }

   public void SetPlayerNearby(bool isNearby)
   {
      isPLayerNearby = isNearby;
      if (!isNearby)
      {
         HidePrompt();
         HideWhiteDot();
      }
   }

   public void HideWhiteDot()
   {
      SetCanvasState(whiteDotCanvas, false);
   }

   public void ShowWhiteDot()
   {
      if (isPLayerNearby)
         SetCanvasState(whiteDotCanvas, true);
   }
   void SetCanvasState(Canvas canvas, bool state)
   {
      if (canvas != null && canvas.gameObject.activeSelf != state)
         canvas.gameObject.SetActive(state);
   }

}

