using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FilePickup : MonoBehaviour, InteractableScript, stage2_InteractableScript
{
    [Header("UI Display")]
    [SerializeField] private GameObject fileUIObject;
    [SerializeField] private GameObject pressAnyKeyText;

    [Header("File Settings")]
    [SerializeField] private Sprite fileSprite; // The file image to display
    [SerializeField] private bool hidePhysicalObjectWhileReading = true; // Hide the 3D object while reading

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float inputDelay = 0.3f; // Delay before accepting input to close

    [Header("Optional Dialogue")]
    [SerializeField] private DialogueTrigger dialogueTrigger; // Optional dialogue when picked up

    private Image fileImage;
    private bool isDisplaying = false;
    private float displayStartTime;
    private bool physicalObjectHidden = false;

    void Start()
    {
        if (fileUIObject != null)
        {
            fileUIObject.SetActive(false);
            fileImage = fileUIObject.GetComponent<Image>();

            if (fileImage != null)
            {
                Color color = fileImage.color;
                color.a = 0f;
                fileImage.color = color;

                // Set the sprite if provided
                if (fileSprite != null)
                {
                    fileImage.sprite = fileSprite;
                }
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
                    HideFile();
                }
            }
        }
    }

    public void InteractScript()
    {
        Debug.Log($"File picked up: {gameObject.name}");

        // Play optional dialogue
        if (dialogueTrigger != null)
        {
            dialogueTrigger.TriggerNow();
        }

        // Show the file UI
        if (fileUIObject != null)
        {
            fileUIObject.SetActive(true);
            isDisplaying = true;
            displayStartTime = Time.time;

            if (fileImage != null)
            {
                StartCoroutine(FadeIn());
            }
        }

        if (pressAnyKeyText != null)
            pressAnyKeyText.SetActive(true);

        // Optionally hide the physical object while reading
        if (hidePhysicalObjectWhileReading)
        {
            HidePhysicalObject();
        }

        // NEW: Re-add this object to the nearby list after a frame
        StartCoroutine(ReAddToNearbyList());
    }

    // NEW: Re-add to nearby list after interaction
    private IEnumerator ReAddToNearbyList()
    {
        yield return null; // Wait one frame

        if (PlayerInteraction.instance != null)
        {
            InteractableObject interactableObj = GetComponent<InteractableObject>();
            if (interactableObj != null)
            {
                PlayerInteraction.instance.AddNearbyObject(interactableObj);
            }
        }
        else if (stage2_interaction.instance != null)
        {
            stage2_interactableOBJ interactableObj = GetComponent<stage2_interactableOBJ>();
            if (interactableObj != null)
            {
                stage2_interaction.instance.AddNearbyObject(interactableObj);
            }
        }
    }

    void HidePhysicalObject()
    {
        // Hide renderers but keep collider active so you can pick it up again
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        physicalObjectHidden = true;
    }

    void ShowPhysicalObject()
    {
        // Show renderers again
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        physicalObjectHidden = false;
    }

    void HideFile()
    {
        Debug.Log("Hiding file UI");
        isDisplaying = false;

        if (fileUIObject != null)
            fileUIObject.SetActive(false);

        if (pressAnyKeyText != null)
            pressAnyKeyText.SetActive(false);

        // Show the physical object again if it was hidden
        if (physicalObjectHidden)
        {
            ShowPhysicalObject();
        }
    }

    IEnumerator FadeIn()
    {
        if (fileImage == null)
            yield break;

        float elapsedTime = 0f;
        Color color = fileImage.color;

        // Start fully transparent
        color.a = 0f;
        fileImage.color = color;

        // Fade in
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            fileImage.color = color;
            yield return null;
        }

        // Ensure fully opaque at end
        color.a = 1f;
        fileImage.color = color;
    }
}