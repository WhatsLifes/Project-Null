using TMPro;
using UnityEngine;

interface InteractableScript
{
   public void InteractScript();
}

public class InteractableObject : MonoBehaviour
{
   [SerializeField] private Canvas whiteDotCanvas;
   [SerializeField] private Canvas promptCanvas;

   [SerializeField] private AudioSource audioSource;
   [SerializeField] private AudioClip interactSound;

   private bool isPLayerNearby = false;

   public bool keep = false;
   public bool IsPLayerNearby => isPLayerNearby;

   void Awake()
   {
      DontDestroyOnLoad(gameObject);
   }

   private void Start()
   {
      if (audioSource == null)
         audioSource = GetComponent<AudioSource>();

      SetCanvasState(whiteDotCanvas, false);
      SetCanvasState(promptCanvas, false);
   }

   public virtual void InteractItem()
   {
      InteractableScript TheInteractScript = gameObject.GetComponent<InteractableScript>();

      if (TheInteractScript == null)
         return;

      HidePrompt();
      HideWhiteDot();

      if (!keep && PlayerInteraction.instance != null)
      {
         PlayerInteraction.instance.RemoveNearbyObject(this);
      }

      TheInteractScript.InteractScript();

      if (audioSource != null && interactSound != null)
      {
         audioSource.PlayOneShot(interactSound);
      }
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