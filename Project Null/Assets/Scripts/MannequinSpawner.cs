using UnityEngine;

public class MannequinSpawner : MonoBehaviour
{
    [Header("Mannequin Settings")]
    [Tooltip("Drag all mannequin GameObjects that should spawn when player enters")]
    public GameObject[] mannequinsToSpawn;

    [Header("Spawn Options")]
    [Tooltip("Should the mannequins only spawn once, or every time player enters?")]
    public bool spawnOnlyOnce = true;

    [Tooltip("Optional: Delay before spawning (in seconds)")]
    public float spawnDelay = 0f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private bool hasSpawned = false;

    void Start()
    {
        // Hide all mannequins at the start
        HideMannequins();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if player entered the trigger
        if (other.CompareTag("Player"))
        {
            // If spawn only once and already spawned, do nothing
            if (spawnOnlyOnce && hasSpawned)
            {
                if (showDebugLogs)
                    Debug.Log("[MannequinSpawner] Already spawned - ignoring");
                return;
            }

            if (showDebugLogs)
                Debug.Log("[MannequinSpawner] Player entered trigger zone!");

            // Spawn the mannequins
            if (spawnDelay > 0)
            {
                Invoke(nameof(SpawnMannequins), spawnDelay);
            }
            else
            {
                SpawnMannequins();
            }

            hasSpawned = true;
        }
    }

    private void HideMannequins()
    {
        if (mannequinsToSpawn == null || mannequinsToSpawn.Length == 0)
        {
            if (showDebugLogs)
                Debug.LogWarning("[MannequinSpawner] No mannequins assigned!");
            return;
        }

        foreach (GameObject mannequin in mannequinsToSpawn)
        {
            if (mannequin != null)
            {
                mannequin.SetActive(false);
            }
        }

        if (showDebugLogs)
            Debug.Log($"[MannequinSpawner] Hid {mannequinsToSpawn.Length} mannequins at start");
    }

    private void SpawnMannequins()
    {
        if (mannequinsToSpawn == null || mannequinsToSpawn.Length == 0) return;

        int spawnedCount = 0;
        foreach (GameObject mannequin in mannequinsToSpawn)
        {
            if (mannequin != null)
            {
                mannequin.SetActive(true);
                spawnedCount++;
            }
        }

        if (showDebugLogs)
            Debug.Log($"[MannequinSpawner] Spawned {spawnedCount} mannequins!");
    }

    // Optional: Method to reset spawning if needed from other scripts
    public void ResetSpawner()
    {
        hasSpawned = false;
        HideMannequins();
    }
}