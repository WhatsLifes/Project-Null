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
    private static GameObject activePickup; // Keep reference to pickup

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
        // Check if THIS is the active pickup and picture is displaying
        if (activePickup == this.gameObject && isDisplaying)
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

        // Set this as the active pickup
        activePickup = this.gameObject;

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

        // Hide the pickup visually but DON'T destroy yet
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    void HidePicture()
    {
        isDisplaying = false;

        if (pictureUIObject != null)
        {
            pictureUIObject.SetActive(false);
        }

        if (pressAnyKeyText != null)
        {
            pressAnyKeyText.SetActive(false);
        }

        // NOW destroy the pickup
        if (activePickup != null)
        {
            Destroy(activePickup);
            activePickup = null;
        }
    }
}