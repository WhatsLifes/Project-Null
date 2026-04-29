using UnityEngine;

public class CobwebZone : MonoBehaviour
{
    public float slowMultiplier = 0.4f;
    public AudioSource cobwebSound;

    private int playersInside = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SimpleFPS player = other.GetComponent<SimpleFPS>();
            if (player != null)
            {
                playersInside++;

                // Apply slow
                player.speedMultiplier *= slowMultiplier;

                if (cobwebSound != null && !cobwebSound.isPlaying)
                    cobwebSound.Play();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SimpleFPS player = other.GetComponent<SimpleFPS>();
            if (player != null)
            {
                playersInside--;

                // Remove slow
                player.speedMultiplier /= slowMultiplier;

                if (playersInside <= 0)
                {
                    playersInside = 0;

                    if (cobwebSound != null)
                        cobwebSound.Stop();
                }
            }
        }
    }
}