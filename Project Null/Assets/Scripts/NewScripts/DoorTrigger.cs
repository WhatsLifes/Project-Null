using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public DoorController door;
    public AudioSource audioSource;
    public AudioClip audioClip;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            audioSource.PlayOneShot(audioClip);
            door.OpenDoor();
        }
    }
}
