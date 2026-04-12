using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public DoorController door;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            door.OpenDoor();
        }
    }
}
