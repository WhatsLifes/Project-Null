using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChaseAudioManager : MonoBehaviour
{
    public static ChaseAudioManager Instance;

    [Header("Audio Sources & Clips")]
    public AudioSource audioSource;
    public AudioClip chaseClip;
    public AudioClip stingerClip;

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float maxVolume = 1f;
    public float fadeDuration = 1.5f;
    public float minPitch = 1f;
    public float maxPitch = 1.3f;

    private bool stingerPlayed = false;
    private Coroutine fadeCoroutine;
    private HashSet<MannequinEnemy> chasingEnemies = new HashSet<MannequinEnemy>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource.loop = true;
        audioSource.clip = chaseClip;
        audioSource.volume = 0f;
    }

    // ------------------------------
    // METHODS THAT MANNEQUIN CALLS 👇
    // ------------------------------

    public void EnemeyStartedChasing(MannequinEnemy enemy)
    {
        HandleEnemyStarted(enemy);
    }

    public void EnemyStoppedChasing(MannequinEnemy enemy)
    {
        HandleEnemyStopped(enemy);
    }

    // ------------------------------
    // INTERNAL FUNCTIONAL LOGIC 👇
    // ------------------------------

    private void HandleEnemyStarted(MannequinEnemy enemy)
    {
        if (!chasingEnemies.Add(enemy))
            return;

        UpdatePitch();

        if (!audioSource.isPlaying)
        {
            PlayStinger();
            audioSource.PlayDelayed(stingerClip != null ? stingerClip.length : 0f);
        }

        FadeAudio(maxVolume);
    }

    private void HandleEnemyStopped(MannequinEnemy enemy)
    {
        if (!chasingEnemies.Remove(enemy))
            return;

        UpdatePitch();

        if (chasingEnemies.Count == 0)
        {
            FadeAudio(0f);
            StartCoroutine(StopWhenSilent());
            ResetStinger();
        }
    }

    private void PlayStinger()
    {
        if (!stingerPlayed && stingerClip != null)
        {
            audioSource.PlayOneShot(stingerClip);
            stingerPlayed = true;
        }
    }

    private void ResetStinger()
    {
        stingerPlayed = false;
    }

    private void UpdatePitch()
    {
        int count = chasingEnemies.Count;
        float t = Mathf.InverseLerp(1, 5, count);
        audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, t);
    }

    private void FadeAudio(float targetVolume)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine(targetVolume));
    }

    private IEnumerator FadeRoutine(float targetVolume)
    {
        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    private IEnumerator StopWhenSilent()
    {
        while (audioSource.volume > 0.01f)
            yield return null;

        audioSource.Stop();
    }
}
