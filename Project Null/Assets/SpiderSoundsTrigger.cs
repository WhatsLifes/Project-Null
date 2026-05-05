using UnityEngine;

public class SpiderSoundsTrigger : MonoBehaviour
{
    public AudioSource spidersAudioSource;
    
    private void OnTriggerEnter(Collider other)
    {
        // check if object entering collider is Player
        if (other.CompareTag("Player"))
        {
            if (spidersAudioSource != null && !spidersAudioSource.isPlaying)
            {
                spidersAudioSource.Play();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (spidersAudioSource != null)
            {
                spidersAudioSource.Stop();
            }
        }
    }
}