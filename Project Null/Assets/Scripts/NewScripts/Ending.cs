using sc.terrain.proceduralpainter;
using System.Collections;
using TMPro;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;

public class EndingManager : MonoBehaviour
{
    [Header("Ending Scene")]
    [SerializeField] private string endingSceneName = "Ending"; // Name of your ending scene
    [SerializeField] private bool loadEndingScene = false; // If false, just shows canvas
    [SerializeField] private string menuSceneName = "MainMenu"; // Menu scene name

    [Header("Canvas References")]
    [SerializeField] private Canvas endingCanvas;
    [SerializeField] private CanvasGroup fadePanel; // Black screen overlay
    [SerializeField] private CanvasGroup endingTextGroup; // For "THE END" or ending message
    [SerializeField] private TMP_Text endingTitleText; // Optional: "THE END" text
    [SerializeField] private TMP_Text endingMessageText; // Optional: ending message
    [SerializeField] private Button returnToMenuButton; // Return to menu button

    [Header("Player References")]
    [SerializeField] private Player player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private SimpleFPS playerController;
    [SerializeField] private HUD hud;

    [Header("Camera Fall Animation")]
    [SerializeField] private bool useCameraFall = true;
    [SerializeField] private float fallDuration = 2f;
    [SerializeField] private float fallRotationAmount = 90f;
    [SerializeField] private float dropHeight = 1.5f;
    [SerializeField] private float sidewaysDistance = 0.8f;
    [SerializeField] private float forwardDistance = 0.3f;
    [SerializeField] private AnimationCurve fallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip endingMusic;

    [Header("Timing")]
    [SerializeField] private float delayBeforeFall = 2f; // Wait before camera falls
    [SerializeField] private float delayAfterFall = 1f; // Wait after fall before fade
    [SerializeField] private float fadeOutDuration = 3f;
    [SerializeField] private float textFadeInDelay = 1f; // Delay before showing ending text
    [SerializeField] private float textFadeInDuration = 2f;
    [SerializeField] private float buttonFadeInDelay = 2f; // Delay before showing button after text
    [SerializeField] private float buttonFadeInDuration = 1f;

    [Header("Ending Dialogue")]
    [SerializeField] private DialogueSequence endingDialogue;


    [Header("Ending Text")]
    [SerializeField] private string titleText = "THE END";
    [SerializeField] private string messageText = "Thank you for playing";

    private bool endingTriggered = false;

    private void Start()
    {
        // Auto-find references
        if (player == null) player = FindObjectOfType<Player>();
        if (playerController == null) playerController = FindObjectOfType<SimpleFPS>();
        if (hud == null) hud = FindObjectOfType<HUD>();

        if (player != null && playerCamera == null)
        {
            playerCamera = player.GetComponentInChildren<Camera>();
        }

        // Setup ending canvas
        if (endingCanvas != null)
        {
            endingCanvas.gameObject.SetActive(true);
        }

        // Setup fade panel
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.gameObject.SetActive(true);
        }

        // Setup ending text group
        if (endingTextGroup != null)
        {
            endingTextGroup.alpha = 0f;
            endingTextGroup.gameObject.SetActive(false);
        }

        // Set ending text
        if (endingTitleText != null)
        {
            endingTitleText.text = titleText;
        }

        if (endingMessageText != null)
        {
            endingMessageText.text = messageText;
        }

        // Setup button
        if (returnToMenuButton != null)
        {
            returnToMenuButton.gameObject.SetActive(false);
            returnToMenuButton.onClick.AddListener(ReturnToMenu);
        }
    }

    public void TriggerEnding()
    {
        if (endingTriggered) return;

        endingTriggered = true;

        // Prevent normal death system from triggering
        if (player != null)
        {
            player.enabled = false; // Disable Player script to prevent OnDeath event
        }

        StartCoroutine(EndingSequence());
    }

    private IEnumerator EndingSequence()
    {
        // Disable player controls - DISABLE THE ENTIRE COMPONENT
        if (playerController != null)
        {
            playerController.enabled = false;  
        }

        // Hide HUD
        if (hud != null)
        {
            hud.HideAllHUD();
            hud.enabled = false;
        }
    // ▶️ Start Dialogue Sequence First
    if (endingDialogue != null)
    {
        var dm = DialogueManager.Instance;
        if (dm != null)
        {
            dm.StartDialogue(endingDialogue);

            // ⏳ WAIT UNTIL DIALOGUE FINISHES
            while (dm.IsPlaying)
            {
               yield return null;
            }
        }
    }
    else
    {
        Debug.LogWarning("EndingManager has no EndingDialogue assigned!");
    }

        // Play ending music
        if (audioSource != null && endingMusic != null)
        {
            audioSource.PlayOneShot(endingMusic);
        }

        // Wait before camera falls
        yield return new WaitForSeconds(delayBeforeFall);

        // Camera fall animation (if enabled)
        if (useCameraFall && playerCamera != null)
        {
            float elapsed = 0f;

            // Store starting values
            Quaternion startRotation = playerCamera.transform.localRotation;
            Vector3 startPosition = playerCamera.transform.localPosition;

            // Target rotation
            Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, fallRotationAmount);

            // Target position - moves in an arc as if pivoting from feet
            float sidewaysDirection = Mathf.Sign(fallRotationAmount);
            Vector3 offset = new Vector3(
                sidewaysDirection * sidewaysDistance,
                -dropHeight,
                forwardDistance
            );
            Vector3 targetPosition = startPosition + offset;

            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fallDuration;
                float curveT = fallCurve.Evaluate(t);

                // Rotate AND move
                playerCamera.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, curveT);
                playerCamera.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, curveT);

                yield return null;
            }

        }

        // Wait after fall
        yield return new WaitForSeconds(delayAfterFall);

        // Fade to black
        if (fadePanel != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                fadePanel.alpha = Mathf.Clamp01(elapsed / fadeOutDuration);
                yield return null;
            }
            fadePanel.alpha = 1f;
        }

        // Wait before showing ending text
        yield return new WaitForSeconds(textFadeInDelay);

        // Fade in ending text
        if (endingTextGroup != null)
        {
            endingTextGroup.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < textFadeInDuration)
            {
                elapsed += Time.deltaTime;
                endingTextGroup.alpha = Mathf.Clamp01(elapsed / textFadeInDuration);
                yield return null;
            }
            endingTextGroup.alpha = 1f;
        }

        // Wait before showing button
        yield return new WaitForSeconds(buttonFadeInDelay);

        // Show and fade in button
        if (returnToMenuButton != null)
        {
            returnToMenuButton.gameObject.SetActive(true);

            // Get button's CanvasGroup (or Image component for fade)
            CanvasGroup buttonGroup = returnToMenuButton.GetComponent<CanvasGroup>();
            if (buttonGroup == null)
            {
                buttonGroup = returnToMenuButton.gameObject.AddComponent<CanvasGroup>();
            }

            buttonGroup.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < buttonFadeInDuration)
            {
                elapsed += Time.deltaTime;
                buttonGroup.alpha = Mathf.Clamp01(elapsed / buttonFadeInDuration);
                yield return null;
            }
            buttonGroup.alpha = 1f;
        }

        // Show cursor for button interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // If loadEndingScene is true, still load scene after showing everything
        if (loadEndingScene)
        {
            yield return new WaitForSeconds(5f); // Optional: auto-load after 5 seconds
            SceneManager.LoadScene(endingSceneName);
        }
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f; // Reset time scale just in case
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(menuSceneName);
    }
}
