using UnityEngine;
using UnityEngine.UI;

public class PickupDaughterPicture : MonoBehaviour, InteractableScript
{
    [Header("UI Display")]
    [SerializeField] private GameObject pictureUIObject;
    [SerializeField] private GameObject pressAnyKeyText;

    [Header("Game State")]
    public static bool daughterPiecePickedUp = false;

    [Header("ElevatorButton")]
    [SerializeField] private ElevatorButtonFixed elevatorButton;

    private static Image pictureImage;
    private static bool isDisplaying = false;
    private static PickupDaughterPicture activeInstance;

    void Start()
    {
        if (pictureUIObject != null)
        {
            pictureUIObject.SetActive(false);
            pictureImage = pictureUIObject.GetComponent<Image>();

            // Force color to white with full alpha
            if (pictureImage != null)
            {
                pictureImage.color = Color.white;
            }

            // Remove any Canvas Group that might interfere
            CanvasGroup cg = pictureUIObject.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
            }
        }

        if (pressAnyKeyText != null)
        {
            pressAnyKeyText.SetActive(false);
        }
    }

    void Update()
    {
        // Check if THIS is the active instance and picture is displaying
        if (activeInstance == this && isDisplaying)
        {
            // Check for ANY key press (keyboard or mouse)
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                HidePicture();
            }
        }
    }

    public void InteractScript()
    {
        // Set game state
        daughterPiecePickedUp = true;

        if (elevatorButton != null)
            elevatorButton.conditionMet = true;

        Debug.Log("Daughter picture piece picked up!");

        // Set this as the active instance
        activeInstance = this;

        // Show UI
        if (pictureUIObject != null)
        {
            pictureUIObject.SetActive(true);
            isDisplaying = true;

            // FORCE the image to be visible
            if (pictureImage != null)
            {
                pictureImage.enabled = true;
                pictureImage.color = Color.white; // Full white, full alpha

                // Double-check Canvas Group
                CanvasGroup cg = pictureUIObject.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }
        }

        if (pressAnyKeyText != null)
        {
            pressAnyKeyText.SetActive(true);
        }

        // Hide the entire GameObject (but keep Update running)
        // This hides mesh, collider, everything except the script
        HidePickupVisuals();
    }

    void HidePickupVisuals()
    {
        // Disable renderer so it's invisible
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;

        // Also check for skinned mesh renderer
        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        if (smr != null) smr.enabled = false;

        // Disable collider so it can't be picked up again
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // If there are child objects with renderers, hide those too
        foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
        {
            childRenderer.enabled = false;
        }

        Debug.Log("Pickup visuals hidden (GameObject still exists)");
    }

    void HidePicture()
    {
        Debug.Log("Hiding picture UI");

        isDisplaying = false;

        if (pictureUIObject != null)
        {
            pictureUIObject.SetActive(false);
        }

        if (pressAnyKeyText != null)
        {
            pressAnyKeyText.SetActive(false);
        }

        // GameObject stays hidden, Update stops checking for input
        activeInstance = null;
    }

    // Optional: Call this to show the pickup again (if needed for respawning)
    public void ShowPickupVisuals()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = true;

        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        if (smr != null) smr.enabled = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
        {
            childRenderer.enabled = true;
        }

        Debug.Log("Pickup visuals shown");
    }

    // Optional: Reset the pickup state (useful for respawning)
    public void ResetPickup()
    {
        daughterPiecePickedUp = false;
        isDisplaying = false;
        activeInstance = null;

        if (elevatorButton != null)
            elevatorButton.conditionMet = false;

        ShowPickupVisuals();

        Debug.Log("Pickup reset");
    }
}