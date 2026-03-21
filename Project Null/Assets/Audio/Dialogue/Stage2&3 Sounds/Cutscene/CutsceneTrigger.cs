using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CutsceneTrigger : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector timeline;

    [Header("Player")]
    public string playerTag = "Player";
    public MonoBehaviour playerController;
    public GameObject playerModel;
    public Animator playerAnimator;

    private GameObject playerObject;

    [Header("Cameras")]
    public Camera playerCamera;
    public Camera animationCamera;

    [Header("Fade")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;

    [Header("Fade Timing")]
    public float fadeStartTime = 5f; // Fade OUT (to black)
    public float fadeEndTime = 9f;   // Fade IN (back to gameplay)

    private bool hasFadedOut = false;
    private bool hasFadedIn = false;

    [Header("Wake Up")]
    public Transform wakeUpPoint;
    public float blackScreenDelay = 2f;

    [Header("Post Processing")]
    public Volume volume;
    private Vignette vignette;
    private DepthOfField depthOfField;

    [Header("Cutscene Objects")]
    public GameObject[] cutsceneObjects;

    [Header("Settings")]
    public bool playOnlyOnce = true;

    private bool hasPlayed = false;

    private void Start()
    {
        if (timeline != null)
            timeline.Stop();

        if (animationCamera != null)
            animationCamera.enabled = false;

        // Hide cutscene objects
        foreach (var obj in cutsceneObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Start visible
        if (fadeCanvas != null)
            fadeCanvas.alpha = 0;

        // Get post-processing effects
        if (volume != null)
        {
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out depthOfField);
        }
    }

    private void Update()
    {
        if (timeline == null) return;

        if (timeline.state == PlayState.Playing)
        {
            // Fade OUT (to black)
            if (!hasFadedOut && timeline.time >= fadeStartTime)
            {
                hasFadedOut = true;
                StartCoroutine(Fade(0, 1));
            }

            // Fade IN (back to gameplay)
            if (!hasFadedIn && timeline.time >= fadeEndTime)
            {
                hasFadedIn = true;
                StartCoroutine(Fade(1, 0));
                StartCoroutine(RegainVision());
            }
        }
    }

    private void OnEnable()
    {
        if (timeline != null)
        {
            timeline.played += OnCutsceneStart;
            timeline.stopped += OnCutsceneEnd;
        }
    }

    private void OnDisable()
    {
        if (timeline != null)
        {
            timeline.played -= OnCutsceneStart;
            timeline.stopped -= OnCutsceneEnd;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayed && playOnlyOnce) return;

        if (other.CompareTag(playerTag))
        {
            playerObject = other.gameObject;

            timeline.Play();
            hasPlayed = true;
        }
    }

    private void OnCutsceneStart(PlayableDirector pd)
    {
        hasFadedOut = false;
        hasFadedIn = false;

        // Disable player control
        if (playerController != null)
            playerController.enabled = false;

        // Hide player model
        if (playerModel != null)
            playerModel.SetActive(false);

        // Enable cutscene objects
        foreach (var obj in cutsceneObjects)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // Switch cameras
        if (playerCamera != null)
            playerCamera.enabled = false;

        if (animationCamera != null)
            animationCamera.enabled = true;
    }

    private void OnCutsceneEnd(PlayableDirector pd)
    {
        StartCoroutine(WakeUpSequence());
    }

    private IEnumerator WakeUpSequence()
    {
        // Wait while screen is black (timeline handled fade)
        yield return new WaitForSeconds(blackScreenDelay);

        // Move player to wake-up position
        if (playerObject != null && wakeUpPoint != null)
        {
            playerObject.transform.position = wakeUpPoint.position;
            playerObject.transform.rotation = wakeUpPoint.rotation;
        }

        // Switch cameras back
        if (playerCamera != null)
            playerCamera.enabled = true;

        if (animationCamera != null)
            animationCamera.enabled = false;

        // Show player model
        if (playerModel != null)
            playerModel.SetActive(true);

        // Disable cutscene objects
        foreach (var obj in cutsceneObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Optional wake-up animation trigger
        if (playerAnimator != null)
            playerAnimator.SetTrigger("WakeUp");

        // Enable player control AFTER everything
        if (playerController != null)
            playerController.enabled = true;
    }

    private IEnumerator Fade(float start, float end)
    {
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            if (fadeCanvas != null)
                fadeCanvas.alpha = Mathf.Lerp(start, end, t / fadeDuration);

            yield return null;
        }

        if (fadeCanvas != null)
            fadeCanvas.alpha = end;
    }

    private IEnumerator RegainVision()
    {
        float t = 0;
        float duration = 3f;

        if (depthOfField != null)
            depthOfField.focusDistance.value = 0.1f;

        if (vignette != null)
            vignette.intensity.value = 0.4f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;

            if (depthOfField != null)
                depthOfField.focusDistance.value = Mathf.Lerp(0.1f, 10f, lerp);

            if (vignette != null)
                vignette.intensity.value = Mathf.Lerp(0.4f, 0f, lerp);

            yield return null;
        }
    }
}