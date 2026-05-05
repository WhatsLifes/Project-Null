using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroTransition : MonoBehaviour
{
    public float duration = 3f;
    public AudioSource audioSource;

    void Start()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }

        Invoke(nameof(LoadMenu), duration);
    }

    void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}