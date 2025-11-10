using UnityEngine;
using System.Collections;

public class MannequinActivatorTrigger : MonoBehaviour
{
    [System.Serializable]
    public class MannequinPair
    {
        [Tooltip("The static mannequin (will be destroyed)")]
        public GameObject staticMannequin;

        [Tooltip("The animated mannequin (will be activated)")]
        public GameObject animatedMannequin;
    }

    [Header("Mannequin Pairs")]
    [Tooltip("Add pairs of static and animated mannequins to switch")]
    public MannequinPair[] mannequinPairs;

    [Header("Trigger Settings")]
    [Tooltip("Should the mannequins only activate once, or every time player enters?")]
    public bool activateOnlyOnce = true;

    [Tooltip("Optional: Delay before switching (in seconds)")]
    public float switchDelay = 0f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private bool hasActivated = false;

    void Start()
    {
        // Disable all animated mannequins completely at the start
        HideAnimatedMannequins();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if player entered the trigger
        if (other.CompareTag("Player"))
        {
            // If activate only once and already activated, do nothing
            if (activateOnlyOnce && hasActivated)
            {
                if (showDebugLogs)
                    Debug.Log("[MannequinActivatorTrigger] Already activated - ignoring");
                return;
            }

            if (showDebugLogs)
                Debug.Log("[MannequinActivatorTrigger] Player entered trigger zone!");

            // Activate the mannequins
            if (switchDelay > 0)
            {
                StartCoroutine(ActivateWithDelay());
            }
            else
            {
                ActivateMannequins();
            }

            hasActivated = true;
        }
    }

    private void HideAnimatedMannequins()
    {
        if (mannequinPairs == null || mannequinPairs.Length == 0)
        {
            if (showDebugLogs)
                Debug.LogWarning("[MannequinActivatorTrigger] No mannequin pairs assigned!");
            return;
        }

        foreach (MannequinPair pair in mannequinPairs)
        {
            if (pair.animatedMannequin != null)
            {
                // Disable the GameObject completely
                pair.animatedMannequin.SetActive(false);
            }
        }

        if (showDebugLogs)
            Debug.Log($"[MannequinActivatorTrigger] Hid {mannequinPairs.Length} animated mannequins at start");
    }

    private System.Collections.IEnumerator ActivateWithDelay()
    {
        yield return new WaitForSeconds(switchDelay);
        ActivateMannequins();
    }

    private void ActivateMannequins()
    {
        if (mannequinPairs == null || mannequinPairs.Length == 0)
        {
            if (showDebugLogs)
                Debug.LogWarning("[MannequinActivatorTrigger] No mannequin pairs to activate!");
            return;
        }

        int activatedCount = 0;

        foreach (MannequinPair pair in mannequinPairs)
        {
            // Destroy static mannequin
            if (pair.staticMannequin != null)
            {
                if (showDebugLogs)
                    Debug.Log($"[MannequinActivatorTrigger] Destroying static mannequin '{pair.staticMannequin.name}'");

                Destroy(pair.staticMannequin);
            }

            // Activate animated mannequin
            if (pair.animatedMannequin != null)
            {
                // Enable the GameObject - this triggers Awake() and Start()
                pair.animatedMannequin.SetActive(true);

                if (showDebugLogs)
                    Debug.Log($"[MannequinActivatorTrigger] Activated animated mannequin '{pair.animatedMannequin.name}'");

                // Wait a frame for Start() to complete, then manually call Activate()
                StartCoroutine(InitializeMannequin(pair.animatedMannequin));

                activatedCount++;
            }
        }

        if (showDebugLogs)
            Debug.Log($"[MannequinActivatorTrigger] Activated {activatedCount} mannequin pairs!");
    }

    private IEnumerator InitializeMannequin(GameObject mannequin)
    {
        // Wait for Start() and other initialization to complete
        yield return new WaitForSeconds(1f); // Wait for StartPatrolling coroutine (0.5s) + buffer

        // Manually activate the Enemy AI
        Enemy enemyScript = mannequin.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.Activate();

            if (showDebugLogs)
                Debug.Log($"[MannequinActivatorTrigger] Manually activated Enemy AI for '{mannequin.name}' - should be patrolling now");
        }
    }

    // Optional: Method to reset the trigger if needed from other scripts
    public void ResetTrigger()
    {
        hasActivated = false;
    }
}