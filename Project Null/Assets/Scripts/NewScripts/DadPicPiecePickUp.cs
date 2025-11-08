using UnityEngine;
using UnityEngine.UI;

public class PickupPicture : MonoBehaviour, InteractableScript
{
    [Header("UI Display")]
    [SerializeField] private GameObject pictureUIObject; // The UI Image GameObject
    [SerializeField] private float displayDuration = 3f; // How long to show (0 = permanent)

    [Header("Game State")]
    public static bool dadPiecePickedUp = false;

    private Image pictureImage;

    void Start()
    {
        // Make sure the UI is hidden at start
        if (pictureUIObject != null)
        {
            pictureUIObject.SetActive(false);
            pictureImage = pictureUIObject.GetComponent<Image>();
        }
    }

    public void InteractScript()
    {
        // Set the bool to true
        dadPiecePickedUp = true;
        Debug.Log("Dad picture piece picked up! Bool set to true");

        // Show the picture on screen
        if (pictureUIObject != null)
        {
            pictureUIObject.SetActive(true);

            // Optional: Fade in effect
            if (pictureImage != null)
            {
                StartCoroutine(FadeIn());
            }

            // Auto-hide after duration (if not permanent)
            if (displayDuration > 0)
            {
                Invoke("HidePicture", displayDuration);
            }
        }

        // Destroy the physical picture from the world
        Destroy(gameObject);
    }

    void HidePicture()
    {
        if (pictureUIObject != null)
        {
            pictureUIObject.SetActive(false);
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