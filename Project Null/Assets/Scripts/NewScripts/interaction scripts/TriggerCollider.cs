using System.Collections;
using UnityEngine;

public class TriggerCollider : MonoBehaviour
{
    [SerializeField] private InteractableObject interactableObject;
    public AudioSource audioSource;
    public AudioClip audioClip;
    
    // when you enter the collider
    private void OnTriggerEnter(Collider other)
    {
        // check for player
        if (other.CompareTag("Player"))
        {
            interactableObject.SetPlayerNearby(true);  // set us nearby
            interactableObject.ShowWhiteDot();   // show the white dot
            PlayerInteraction.instance.AddNearbyObject(interactableObject);  // adds the object to nearby list
            
            // trigger air disperser sounds
            if (audioSource != null)
            {
                audioSource.clip = audioClip;
                StartCoroutine(FadeIn());
            }
        }
    }

    // when you leave the collider
    private void OnTriggerExit(Collider other)
    {
        // check for player
        if (other.CompareTag("Player"))
        {
            interactableObject.SetPlayerNearby(false); // set us *not* nearby
            interactableObject.HideWhiteDot();  // hide the white dot
            interactableObject.HidePrompt();  // make sure we dont see the prompt
            PlayerInteraction.instance.RemoveNearbyObject(interactableObject);  // remove the object from nearby list
        
            // stop air disperser sounds
            if (audioSource != null)
                StartCoroutine(FadeOut());
        }
    }

    // audio fade effects
    IEnumerator FadeIn()
    {
        audioSource.Play();
        float t = 0;

        while (t < 1.5f)
        {
            audioSource.volume = Mathf.Lerp(0, 0.45f, t / 1.5f);
            t += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 0.45f;
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        float startVol = audioSource.volume;

        while (t < 1.5f)
        {
            audioSource.volume = Mathf.Lerp(startVol, 0, t / 1.5f);
            t += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();
    }
}

