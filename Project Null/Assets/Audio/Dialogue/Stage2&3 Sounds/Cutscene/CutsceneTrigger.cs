using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneTrigger : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector timeline;

    [Header("Player")]
    public string playerTag = "Player";
    public MonoBehaviour playerController;
    public GameObject playerModel; // the visible mesh/model of the player

    [Header("Cameras")]
    public Camera playerCamera;
    public Camera animationCamera;

    [Header("Cutscene Objects")]
    public GameObject[] cutsceneObjects; // objects used only during cutscene

    [Header("Settings")]
    public bool playOnlyOnce = true;
    public float resumeDelay = 3f; // delay before gameplay resumes

    private bool hasPlayed = false;

    private void Start()
    {
        if (timeline != null)
            timeline.Stop();

        if (animationCamera != null)
            animationCamera.enabled = false;

        // Ensure cutscene objects start hidden
        foreach (var obj in cutsceneObjects)
        {
            if (obj != null)
                obj.SetActive(false);
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
            timeline.Play();
            hasPlayed = true;
        }
    }

    private void OnCutsceneStart(PlayableDirector pd)
    {
        // Disable player control
        if (playerController != null)
            playerController.enabled = false;

        // Hide player model
        if (playerModel != null)
            playerModel.SetActive(false);

        // Enable cutscene-only objects
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
        StartCoroutine(ResumeAfterDelay());
    }

    private IEnumerator ResumeAfterDelay()
    {
        // wait for delay
        yield return new WaitForSeconds(resumeDelay);

        // Re-enable player control
        if (playerController != null)
            playerController.enabled = true;

        // Show player model again
        if (playerModel != null)
            playerModel.SetActive(true);

        // Disable cutscene-only objects
        foreach (var obj in cutsceneObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Switch cameras back
        if (playerCamera != null)
            playerCamera.enabled = true;

        if (animationCamera != null)
            animationCamera.enabled = false;
    }
}