using UnityEngine;
using System.Collections.Generic;

public class StartingRoomPickupManager : MonoBehaviour
{
    public static StartingRoomPickupManager Instance;

    [Header("Door to open")]
    public DoorController door; // Assign the starting room door pivot

    [Header("Pickups in the room")]
    public List<GameObject> pickups; // Drag your 3 pickable items here

    private HashSet<GameObject> collectedItems = new HashSet<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    public void ItemPickedUp(GameObject item)
    {
        if (!collectedItems.Contains(item))
        {
            collectedItems.Add(item);
            Debug.Log($"Picked up: {item.name} ({collectedItems.Count}/{pickups.Count})");

            if (collectedItems.Count >= pickups.Count)
            {
                OpenDoor();
            }
        }
    }

    private void OpenDoor()
    {
        if (door != null)
        {
            door.OpenDoor();
            Debug.Log("All starting room items collected! Door opening!");
        }
        else
        {
            Debug.LogWarning("StartingRoomPickupManager: door not assigned!");
        }
    }
}
