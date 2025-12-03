using System.Collections;
using UnityEngine;

public class RoomAudioTrigger : MonoBehaviour
{
    private static RoomAudioTrigger previousActiveAudio;
    private static RoomAudioTrigger currentActiveAudio;
    public int priority = 0;
    private AudioSource audioSource;
    public float fadeTime = 1.5f;
    public float maxVolume = 1.0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentActiveAudio != null && currentActiveAudio != this && currentActiveAudio.priority <= priority)
            {
                previousActiveAudio = currentActiveAudio;
                previousActiveAudio.StartCoroutine(previousActiveAudio.FadeOut());
            }
            
            currentActiveAudio = this;
            StartCoroutine(FadeIn());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentActiveAudio == this)
            {
                StartCoroutine(FadeOut());
                currentActiveAudio = null;

                if (previousActiveAudio != null)
                {
                    previousActiveAudio.StartCoroutine(previousActiveAudio.FadeIn());
                    currentActiveAudio = previousActiveAudio;
                    previousActiveAudio = null;
                }
            }
        }
    }

    IEnumerator FadeIn()
    {
        audioSource.Play();
        float t = 0;

        while (t < fadeTime)
        {
            audioSource.volume = Mathf.Lerp(0, maxVolume, t / fadeTime);
            t += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = maxVolume;
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        float startVol = audioSource.volume;

        while (t < fadeTime)
        {
            audioSource.volume = Mathf.Lerp(startVol, 0, t / fadeTime);
            t += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();
    }
}
