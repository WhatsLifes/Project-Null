using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PickupSonPicture : MonoBehaviour, InteractableScript
{
    [Header("UI Display")]
    [SerializeField] private GameObject pictureUIObject; // The UI Image GameObject
    [SerializeField] private GameObject pressAnyKeyText; // Optional "Press any key" prompt

    [Header("Game State")]
    public static bool sonPiecePickedUp = false;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private static Image pictureImage;
    private static bool isDisplaying = false;
    private static PickupSonPicture activeInstance;

    void Start()
    {
        // Make sure the UI is hidden at start
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
        // Set the bool to true
        sonPiecePickedUp = true;

        Debug.Log("Son picture piece picked up! Bool set to true");

        // Set this as the active instance
        activeInstance = this;

        // Show the picture on screen
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

                // Fade in effect
                StartCoroutine(FadeIn());
            }
        }

        // Show "press any key" prompt if you have one
        if (pressAnyKeyText != null)
        {
            pressAnyKeyText.SetActive(true);
        }

        // Hide the pickup visually but DON'T destroy yet (so Update keeps running)
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

        Debug.Log("Son picture pickup visuals hidden");
    }

    void HidePicture()
    {
        Debug.Log("Hiding son picture UI");

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

    // Fade-in effect
    IEnumerator FadeIn()
    {
        if (pictureImage == null) yield break;

        float elapsedTime = 0f;
        Color color = pictureImage.color;

        // Start from transparent
        color.a = 0f;
        pictureImage.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            pictureImage.color = color;
            yield return null;
        }

        // Ensure it's fully visible at the end
        color.a = 1f;
        pictureImage.color = color;
    }

    // Optional: Reset the pickup (for respawning if needed)
    public void ResetPickup()
    {
        sonPiecePickedUp = false;
        isDisplaying = false;
        activeInstance = null;

        ShowPickupVisuals();

        Debug.Log("Son picture pickup reset");
    }

    // Optional: Show pickup again
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
    }
}