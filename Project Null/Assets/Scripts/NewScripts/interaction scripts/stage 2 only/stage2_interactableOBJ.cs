using TMPro;
using UnityEngine;

// think of this like a struct, but anything with this must implement whats in here
interface stage2_InteractableScript
{
    public void InteractScript();
}

public class stage2_interactableOBJ : MonoBehaviour
{
    [Header("UI Elements")] 
    [SerializeField] public Canvas whiteDotCanvas;  
    [SerializeField] public Canvas promptCanvas;  
    
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

    public void InteractItem()
    {
        // gets the object we are trying to interact with
        stage2_InteractableScript TheInteractScript = gameObject.GetComponent<stage2_InteractableScript>();
        stage2_interaction.instance.RemoveNearbyObject(this);  // take the object out of the nearby interact list
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