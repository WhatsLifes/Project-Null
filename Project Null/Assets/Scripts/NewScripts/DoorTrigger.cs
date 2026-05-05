using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("References")]
    public DoorController door;
    public AudioSource audioSource;
    public AudioClip audioClip;

    private void OnTriggerEnter(Collider other)
    {
        // Detect player using tag (more reliable than layer)
        if (!other.CompareTag("Player") && !other.GetComponentInParent<Transform>().CompareTag("Player"))
            return;

        Debug.Log("Player entered trigger: " + other.name);

        // Safety check for door reference
        if (door == null)
        {
            Debug.LogError("Door reference is NULL on " + gameObject.name);
            return;
        }

        // Play sound safely
        if (audioSource != null && audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
        }

        door.OpenDoor();
    }
}