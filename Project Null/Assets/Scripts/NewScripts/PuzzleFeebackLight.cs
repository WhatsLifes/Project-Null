using System.Collections;
using UnityEngine;

public class PuzzleFeedbackLight : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How many times the light should blink")]
    public int blinkCount = 3;

    [Tooltip("How bright the light gets during each flash")]
    public float maxIntensity = 3f;

    [Tooltip("How fast the light fades in/out (higher = faster)")]
    public float fadeSpeed = 25f;

    [Tooltip("Small delay between each blink")]
    public float offDelay = 0.05f;

    [Tooltip("Color when puzzle is correct")]
    public Color correctColor = Color.green;

    [Tooltip("Color when puzzle is incorrect")]
    public Color incorrectColor = Color.red;

    [Header("Optional Sound")]
    public AudioSource audioSource;        // Optional: assign a source in Inspector
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
            _light.enabled = false;
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
            _light.enabled = true;

            // Fade in
            for (float t = 0; t < 1f; t += Time.deltaTime * fadeSpeed)
            {
                _light.intensity = Mathf.Lerp(0f, maxIntensity, t);
                yield return null;
            }

            // Fade out
            for (float t = 0; t < 1f; t += Time.deltaTime * fadeSpeed)
            {
                _light.intensity = Mathf.Lerp(maxIntensity, 0f, t);
                yield return null;
            }

            _light.enabled = false;
            yield return new WaitForSeconds(offDelay);
        }
    }
}