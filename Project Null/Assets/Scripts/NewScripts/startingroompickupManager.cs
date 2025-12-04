using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StartingRoomPickupManager : MonoBehaviour
{
    public static StartingRoomPickupManager Instance;

    [Header("Door to open")]
    public DoorController door; // Assign the starting room door pivot

    [Header("Pickups in the room")]
    public List<GameObject> pickups; // Drag your 2 pickable items here (arm + flashlight)

    private HashSet<GameObject> collectedItems = new HashSet<GameObject>();
    private bool doorOpened = false; // Ensures door only opens once

    [Header("HUD Settings")]
    [SerializeField] private HUD hud;
    [Tooltip("Should this door opening trigger an objective change?")]
    public bool changeObjectiveOnOpen = true;

    [Header("Timer Settings (Scene 3 Only)")]
    [Tooltip("Should opening this door start the stage timer? (Enable for Scene 3)")]
    public bool startTimerOnOpen = false;
    [SerializeField] private StageTimer stageTimer;

    [Header("Door Delay Settings")]
    public float doorOpenDelay = 5f; // Seconds to wait before opening after last item

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Auto-find timer if not assigned and we need it
        if (startTimerOnOpen && stageTimer == null)
        {
            stageTimer = FindObjectOfType<StageTimer>();
        }
    }

    // Call this method when an item is picked up
    public void ItemPickedUp(GameObject item)
    {
        if (doorOpened)
            return; // Door already opened, ignore extra pickups

        if (!pickups.Contains(item))
        {
            Debug.LogWarning($"Picked up item {item.name} is not tracked by StartingRoomPickupManager!");
            return;
        }

        if (collectedItems.Contains(item))
        {
            // Already counted
            return;
        }

        collectedItems.Add(item);
        Debug.Log($"Picked up: {item.name} ({collectedItems.Count}/{pickups.Count})");

        // Check if all required items are collected
        if (collectedItems.Count >= pickups.Count)
        {
            StartCoroutine(OpenDoorAfterDelay());
        }
    }

    private IEnumerator OpenDoorAfterDelay()
    {
        Debug.Log($"All starting room items collected! Waiting {doorOpenDelay} seconds before opening the door...");
        yield return new WaitForSeconds(doorOpenDelay);
        OpenDoor();
    }

    private void OpenDoor()
    {
        if (door != null)
        {
            door.OpenDoor();
            doorOpened = true;

            // Only show objective if enabled
            if (changeObjectiveOnOpen && hud != null)
            {
                hud.ShowObjective2();
            }

            // Start timer if enabled (Scene 3 only)
            if (startTimerOnOpen && stageTimer != null)
            {
                stageTimer.StartTimer();
                Debug.Log("Door opened - Stage timer started!");
            }

            Debug.Log("All starting room items collected! Door opening!");
        }
        else
        {
            Debug.LogWarning("StartingRoomPickupManager: door not assigned!");
        }
    }
}