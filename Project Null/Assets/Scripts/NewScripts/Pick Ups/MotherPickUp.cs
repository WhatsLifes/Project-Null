using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PickupMotherPicture : MonoBehaviour, stage2_InteractableScript
{
    [Header("UI Display")]
    [SerializeField] private GameObject pictureUIObject;
    [SerializeField] private GameObject pressAnyKeyText;

    [Header("Game State")]
    public static bool motherPiecePickedUp = false;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float inputDelay = 0.3f; // Delay before accepting input to close

    [Header("Dialogue Trigger")]
    public DialogueTrigger dialogueTrigger;

    private Image pictureImage;
    private bool isDisplaying = false;
    private float displayStartTime;

    void Start()
    {
        if (pictureUIObject != null)
        {
            pictureUIObject.SetActive(false);
            pictureImage = pictureUIObject.GetComponent<Image>();

            if (pictureImage != null)
            {
                Color color = pictureImage.color;
                color.a = 0f;
                pictureImage.color = color;
            }
        }

        if (pressAnyKeyText != null)
            pressAnyKeyText.SetActive(false);
    }

    void Update()
    {
        if (isDisplaying)
        {
            // Only accept input after the delay
            if (Time.time >= displayStartTime + inputDelay)
            {
                if (Input.anyKeyDown ||
                    Input.GetMouseButtonDown(0) ||
                    Input.GetMouseButtonDown(1))
                {
                    Debug.Log("Key pressed - hiding picture");
                    HidePicture();
                }
            }
        }
    }

    public void InteractScript()
    {
        Debug.Log("=== MOTHER PICTURE INTERACT SCRIPT CALLED ===");

        motherPiecePickedUp = true;
        Debug.Log("Mother picture picked up!");

        // Play the dialogue line
        if (dialogueTrigger != null)
        {
            Debug.Log("Triggering pickup dialogue...");
            dialogueTrigger.TriggerNow();
        }
        else
        {
            Debug.LogWarning("PickupMotherPicture has NO DialogueTrigger assigned!");
        }

        // Show the picture UI
        if (pictureUIObject != null)
        {
            pictureUIObject.SetActive(true);
            isDisplaying = true;
            displayStartTime = Time.time;

            if (pictureImage != null)
            {
                StartCoroutine(FadeIn());
            }
        }

        if (pressAnyKeyText != null)
            pressAnyKeyText.SetActive(true);

        HidePickupVisuals();
    }

    void HidePickupVisuals()
    {
        Debug.Log("Hiding pickup visuals (the 3D object)");
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    void HidePicture()
    {
        Debug.Log("=== HIDING PICTURE ===");
        isDisplaying = false;

        if (pictureUIObject != null)
            pictureUIObject.SetActive(false);

        if (pressAnyKeyText != null)
            pressAnyKeyText.SetActive(false);
    }

    IEnumerator FadeIn()
    {
        if (pictureImage == null)
            yield break;

        float elapsedTime = 0f;
        Color color = pictureImage.color;

        // Start fully transparent
        color.a = 0f;
        pictureImage.color = color;

        // Fade in
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            pictureImage.color = color;
            yield return null;
        }

        // Ensure fully opaque at end
        color.a = 1f;
        pictureImage.color = color;
        Debug.Log("Fade complete - alpha set to 1");
    }
}