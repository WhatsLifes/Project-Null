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
using UnityEngine.Video; // Video playback
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

    [Header("Video Cutscene")]
    [SerializeField] private VideoPlayer videoPlayer; // Video contains full cutscene (including fall)

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip endingMusic;

    [Header("Timing")]
    [SerializeField] private float textFadeInDelay = 1f;
    [SerializeField] private float textFadeInDuration = 2f;
    [SerializeField] private float buttonFadeInDelay = 2f;
    [SerializeField] private float buttonFadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 3f;

    [Header("Ending Text")]
    [SerializeField] private string titleText = "GAME OVER";
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

        if (player != null)
            player.enabled = false;

        StartCoroutine(EndingSequence());
    }

    private IEnumerator EndingSequence()
    {
        // Pause game time
        Time.timeScale = 0f;

        // Disable player controls
        if (playerController != null)
            playerController.enabled = false;

        // Hide HUD
        if (hud != null)
        {
            hud.HideAllHUD();
            hud.enabled = false;
        }

        // Mute all audio in scene
        AudioListener.pause = true;

        // Allow video audio to play while paused
        if (videoPlayer != null && audioSource != null)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, audioSource);
            audioSource.ignoreListenerPause = true;
        }

        // play video cutscene
        if (videoPlayer != null)
        {
            videoPlayer.Play();

            while (!videoPlayer.isPlaying)
                yield return null;

            while (videoPlayer.isPlaying)
                yield return null;
        }

        // Restore global audio
        AudioListener.pause = false;

        // after video end start ending ui

        if (audioSource != null && endingMusic != null)
        {
            audioSource.PlayOneShot(endingMusic);
        }

        // Fade to black
        if (fadePanel != null)
        {
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadePanel.alpha = Mathf.Clamp01(elapsed / fadeOutDuration);
                yield return null;
            }

            fadePanel.alpha = 1f;
        }

        // Show ending text
        yield return new WaitForSecondsRealtime(textFadeInDelay);

        if (endingTextGroup != null)
        {
            endingTextGroup.gameObject.SetActive(true);

            float elapsed = 0f;

            while (elapsed < textFadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                endingTextGroup.alpha = Mathf.Clamp01(elapsed / textFadeInDuration);
                yield return null;
            }

            endingTextGroup.alpha = 1f;
        }

        // Show button
        yield return new WaitForSecondsRealtime(buttonFadeInDelay);

        if (returnToMenuButton != null)
        {
            returnToMenuButton.gameObject.SetActive(true);

            CanvasGroup buttonGroup = returnToMenuButton.GetComponent<CanvasGroup>();
            if (buttonGroup == null)
                buttonGroup = returnToMenuButton.gameObject.AddComponent<CanvasGroup>();

            buttonGroup.alpha = 0f;

            float elapsed = 0f;

            while (elapsed < buttonFadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                buttonGroup.alpha = Mathf.Clamp01(elapsed / buttonFadeInDuration);
                yield return null;
            }

            buttonGroup.alpha = 1f;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (loadEndingScene)
        {
            yield return new WaitForSecondsRealtime(5f);
            SceneManager.LoadScene(endingSceneName);
        }
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(menuSceneName);
    }
}