using System.Collections;
using UnityEngine;

public class RoomAudioTrigger : MonoBehaviour
{
    private AudioSource audioSource;
    public float fadeTime = 1.5f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeIn());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeOut());
        }
    }

    IEnumerator FadeIn()
    {
        audioSource.Play();
        float t = 0;

        while (t < fadeTime)
        {
            audioSource.volume = Mathf.Lerp(0, 1, t / fadeTime);
            t += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 1;
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
