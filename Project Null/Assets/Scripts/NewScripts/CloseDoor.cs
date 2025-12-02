using UnityEngine;

public class DoorCloseTrigger : MonoBehaviour
{
    [Header("Reference to Door Pivot")]
    public DoorController door;  // Drag DoorPivot here

    [Header("References")]
    [Tooltip("Drag the existing AudioSource (e.g. 'SPEAKER') here")]
    public AudioSource speaker; // Assign your speaker AudioSource manually

    [Header("Sound Settings")]
    [Tooltip("Sound that plays when the door closes")]
    public AudioClip closeSound; // Drag any .mp3, .wav, or .ogg sound here

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered door close trigger.");

            if (door != null)
            {
                door.CloseDoor(); // Rotate to 90°
                Debug.Log("CloseDoor() called on: " + door.name);

                // Play sound through the assigned speaker
                if (speaker != null && closeSound != null)
                {
                    speaker.PlayOneShot(closeSound);
                    Debug.Log("Door close sound played through speaker.");
                }
                else if (speaker == null)
                {
                    Debug.LogWarning("No AudioSource (speaker) assigned in DoorCloseTrigger!");
                }
                else if (closeSound == null)
                {
                    Debug.LogWarning("No close sound assigned in DoorCloseTrigger!");
                }
            }
            else
            {
                Debug.LogWarning("Door reference not assigned in DoorCloseTrigger!");
            }
        }
    }
}
