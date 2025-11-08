using UnityEngine;
using UnityEngine.UI;

public class PickupDaughterPicture : MonoBehaviour, InteractableScript
{
    [Header("UI Display")]
    [SerializeField] private GameObject pictureUIObject; // The UI Image GameObject
    [SerializeField] private GameObject pressAnyKeyText; // Optional "Press any key" prompt

    [Header("Game State")]
    public static bool daughterPiecePickedUp = false;

    private Image pictureImage;
    private bool isDisplaying = false;

    void Start()
    {
        // Make sure the UI is hidden at start
        if (pictureUIObject != null)
        {
            pictureUIObject.SetActive(false);
            pictureImage = pictureUIObject.GetComponent<Image>();
        }

        if (pressAnyKeyText != null)
        {
            pressAnyKeyText.SetActive(false);
        }
    }

    void Update()
    {
        // Wait for any key press to close the picture
        if (isDisplaying && Input.anyKeyDown)
        {
            HidePicture();
        }
    }

    public void InteractScript()
    {
        // Set the bool to true
        daughterPiecePickedUp = true;
        Debug.Log("Daughter picture piece picked up! Bool set to true");

        // Show the picture on screen
        if (pictureUIObject != null)
        {
            pictureUIObject.SetActive(true);
            isDisplaying = true;

            // Show "press any key" prompt if you have one
            if (pressAnyKeyText != null)
            {
                pressAnyKeyText.SetActive(true);
            }

            // Optional: Fade in effect
            if (pictureImage != null)
            {
                StartCoroutine(FadeIn());
            }
        }

        // Destroy the physical picture from the world
        Destroy(gameObject);
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
    }

    // Optional fade-in effect
    System.Collections.IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        float fadeDuration = 0.5f;
        Color color = pictureImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            pictureImage.color = color;
            yield return null;
        }
    }
}