using System.Collections;
using UnityEngine;

public class PuzzleFeedbackLight : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How many times the light should blink")]
    public int blinkCount = 3;

    [Tooltip("How bright the light gets during each flash")]
    public float maxIntensity = 3f;

    [Tooltip("Duration of fade in/out in seconds")]
    public float fadeDuration = 0.2f;

    [Tooltip("Small delay between each blink")]
    public float offDelay = 0.1f;

    [Tooltip("Color when puzzle is correct")]
    public Color correctColor = Color.green;

    [Tooltip("Color when puzzle is incorrect")]
    public Color incorrectColor = Color.red;

    [Header("Optional Sound")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip incorrectSound;

    private Light _light;
    private Coroutine flashRoutine;

    void Awake()
    {
        _light = GetComponent<Light>();
        if (_light == null)
        {
            Debug.LogError("PuzzleFeedbackLight: No Light component found!");
        }
        else
        {
            _light.intensity = 0f;
        }
    }

    public void Flash(bool correct)
    {
        if (_light == null) return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine(correct));
    }

    private IEnumerator FlashRoutine(bool correct)
    {
        Debug.Log($"Starting flash - Correct: {correct}, Color: {(correct ? "Green" : "Red")}");

        _light.color = correct ? correctColor : incorrectColor;

        // Optional audio feedback
        if (audioSource != null)
        {
            AudioClip clipToPlay = correct ? correctSound : incorrectSound;
            if (clipToPlay != null)
                audioSource.PlayOneShot(clipToPlay);
        }

        for (int i = 0; i < blinkCount; i++)
        {
            Debug.Log($"Blink {i + 1}/{blinkCount}");

            // Fade in
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                _light.intensity = Mathf.Lerp(0f, maxIntensity, t);
                yield return null;
            }
            _light.intensity = maxIntensity; // Ensure we hit max
            Debug.Log($"Fade in complete - Intensity: {_light.intensity}");

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                _light.intensity = Mathf.Lerp(maxIntensity, 0f, t);
                yield return null;
            }
            _light.intensity = 0f; // Ensure we hit 0
            Debug.Log($"Fade out complete - Intensity: {_light.intensity}");

            if (i < blinkCount - 1)
                yield return new WaitForSeconds(offDelay);
        }

        Debug.Log("Flash routine complete");
    }
}