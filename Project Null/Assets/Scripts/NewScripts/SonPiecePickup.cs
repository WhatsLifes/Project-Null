using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PickupSonPicture : MonoBehaviour, stage2_InteractableScript
{
    [Header("UI Display")]
    [SerializeField] private GameObject pictureUIObject;
    [SerializeField] private GameObject pressAnyKeyText;

    [Header("Game State")]
    public static bool sonPiecePickedUp = false;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float inputDelay = 0.3f;

    [Header("Dialogue Trigger")]
    public DialogueTrigger dialogueTrigger;

    [Header("HUD Reference")] // NEW
    [SerializeField] private HUD hud;

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

        // NEW: Auto-find HUD if not assigned
        if (hud == null)
        {
            hud = FindObjectOfType<HUD>();
        }
    }

    void Update()
    {
        if (isDisplaying)
        {
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
        Debug.Log("=== INTERACT SCRIPT CALLED ===");

        sonPiecePickedUp = true;
        Debug.Log("Son picture picked up!");

        // Update Stage2 Progress
        if (Stage2ProgressManager.Instance != null)
        {
            Stage2ProgressManager.Instance.sonPhotoPickedUp = true;
            Debug.Log("Stage2ProgressManager updated: sonPhotoPickedUp = true");
        }

        // NEW: Show Objective 9
        if (hud != null)
        {
            hud.ShowObjective9();
            Debug.Log("Objective 9 shown!");
        }
        else
        {
            Debug.LogWarning("HUD reference is null - cannot show objective!");
        }

        // Play the dialogue line
        if (dialogueTrigger != null)
        {
            Debug.Log("Triggering pickup dialogue...");
            dialogueTrigger.TriggerNow();
        }
        else
        {
            Debug.LogWarning("PickupSonPicture has NO DialogueTrigger assigned!");
        }

        // Show the picture
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

        color.a = 0f;
        pictureImage.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            pictureImage.color = color;
            yield return null;
        }

        color.a = 1f;
        pictureImage.color = color;
        Debug.Log("Fade complete - alpha set to 1");
    }
}