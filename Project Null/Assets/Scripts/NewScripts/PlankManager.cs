using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlankManager : MonoBehaviour
{
    [Header("All plank GameObjects in scene")]
    public GameObject[] allPlanks;

    [Header("Audio")]
    public AudioSource sfxAudioSource;          // sfx audio source for stop/go sfx
    public AudioClip stopSFX;                   // buzzer sound to stop music
    public AudioClip goSFX;                     // ding sound to play music again
    public GameObject musicSpeaker;             // speaker object where doll theme music will come from

    [Range(0f, 1f)]
    [Tooltip("Fraction of planks that disappear on red light (e.g., 0.4 = 40%)")]
    public float disappearFraction = 0.4f;

    private List<GameObject> currentlyHidden = new List<GameObject>();

    public void OnRedLight()
    {
        Debug.Log("Red light triggered");
        sfxAudioSource.PlayOneShot(stopSFX);
        AudioSource green_music = musicSpeaker.GetComponent<AudioSource>();
        green_music.Pause();

        RestoreAll();

        List<GameObject> shuffled = new List<GameObject>(allPlanks);

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        int hideCount = Mathf.RoundToInt(shuffled.Count * disappearFraction);

        for (int i = 0; i < hideCount; i++)
        {
            shuffled[i].SetActive(false);
            currentlyHidden.Add(shuffled[i]);
        }
    }

    public void OnGreenLight()
    {
        Debug.Log("Green light triggered");
        sfxAudioSource.PlayOneShot(goSFX);
        AudioSource green_music = musicSpeaker.GetComponent<AudioSource>();
        green_music.Play();

        RestoreAll();
    }

    private void RestoreAll()
    {
        foreach (GameObject plank in currentlyHidden)
        {
            if (plank != null)
                plank.SetActive(true);
        }
        currentlyHidden.Clear();
    }
}