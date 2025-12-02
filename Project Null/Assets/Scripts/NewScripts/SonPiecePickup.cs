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

    [Header("Dialogue Trigger")]
    public DialogueTrigger dialogueTrigger;  // <-- ADD THIS

    private static Image pictureImage;
    private static bool isDisplaying = false;
    private static PickupSonPicture activeInstance;

    void Start()
    {
        if (pictureUIObject != null)
        {
            pictureUIObject.SetActive(false);
            pictureImage = pictureUIObject.GetComponent<Image>();

            if (pictureImage != null)
                pictureImage.color = Color.white;

            CanvasGroup cg = pictureUIObject.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
        }

        if (pressAnyKeyText != null)
            pressAnyKeyText.SetActive(false);
    }

    void Update()
    {
        if (activeInstance == this && isDisplaying)
        {
            if (Input.anyKeyDown ||
                Input.GetMouseButtonDown(0) ||
                Input.GetMouseButtonDown(1))
            {
                HidePicture();
            }
        }
    }

    public void InteractScript()
    {
        sonPiecePickedUp = true;
        Debug.Log("Son picture picked up!");

        // 🔥 Update Stage2 Progress
        if (Stage2ProgressManager.Instance != null)
        {
            Stage2ProgressManager.Instance.sonPhotoPickedUp = true;
            Debug.Log("Stage2ProgressManager updated: sonPhotoPickedUp = true");
        }

        // 🔥 PLAY THE DIALOGUE LINE HERE
        if (dialogueTrigger != null)
        {
            Debug.Log("Triggering pickup dialogue...");
            dialogueTrigger.TriggerNow();
        }
        else
        {
            Debug.LogWarning("PickupSonPicture has NO DialogueTrigger assigned!");
        }

        activeInstance = this;

        if (pictureUIObject != null)
        {
            pictureUIObject.SetActive(true);
            isDisplaying = true;

            if (pictureImage != null)
            {
                pictureImage.enabled = true;
                pictureImage.color = Color.white;

                CanvasGroup cg = pictureUIObject.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;

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
        isDisplaying = false;

        if (pictureUIObject != null)
            pictureUIObject.SetActive(false);

        if (pressAnyKeyText != null)
            pressAnyKeyText.SetActive(false);

        activeInstance = null;
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
    }
}
