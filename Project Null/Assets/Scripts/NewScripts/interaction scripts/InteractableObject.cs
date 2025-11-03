using TMPro;
using UnityEngine;

interface InteractableScript
{
   public void InteractScript();
}
public class InteractableObject : MonoBehaviour
{
   [Header("Item Data")]
   [SerializeField] private InteractableItemData itemData;

   [Header("UI Elements")] 
   [SerializeField] private Canvas whiteDotCanvas;  
   [SerializeField] private Canvas promptCanvas;  

   private bool isPLayerNearby = false;
   
   public bool IsPLayerNearby => isPLayerNearby;

  
   private void Start()
   {
      SetCanvasState(whiteDotCanvas, false);
      SetCanvasState(promptCanvas, false);
   }

   public void InteractItem()
   {
      InteractableScript TheInteractScript = gameObject.GetComponent<InteractableScript>();
      PlayerInteraction.instance.RemoveNearbyObject(this);
      HidePrompt();
      HideWhiteDot();

      TheInteractScript.InteractScript();
   }

   public void ShowPrompt()
   {
      HideWhiteDot();
      SetCanvasState(promptCanvas, true);
      
      //itemNameText.text = itemData.itemName;
      //actionText.text = itemData.interactionPrompt;
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
      {
         SetCanvasState(whiteDotCanvas, true);     
      }
   }
   void SetCanvasState(Canvas canvas, bool state)
   {
      if (canvas != null && canvas.gameObject.activeSelf != state)
      {
         canvas.gameObject.SetActive(state);
      }
   }

}

