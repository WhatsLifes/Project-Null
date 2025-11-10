using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UniversalTrigger : MonoBehaviour
{
    [Header("Jumpscare Settings")]
    public GameObject targetPrefab;
    public AudioClip scareSound;
    public float scareDuration = 2.5f; // How long the jumpscare lasts
    public Transform spawnPoint;       // Optional custom spawn point

    [Header("Trigger Options")]
    [Tooltip("If true, waits for player to leave and then re-enter before triggering.")]
    public bool triggerOnSecondPass = false;

    private bool hasActivated = false;
    private bool hasEnteredOnce = false;  // Tracks first entry
    private bool playerInside = false;    // Tracks if the player is currently inside

    private void OnTriggerEnter(Collider other)
    {
        if (hasActivated || !other.CompareTag("Player"))
            return;

        // Normal (single-pass) trigger
        if (!triggerOnSecondPass)
        {
            ActivateTrigger(other);
            return;
        }

        // Second-pass logic
        if (!hasEnteredOnce)
        {
            // First entry (just mark it)
            hasEnteredOnce = true;
            playerInside = true;
            Debug.Log("[UniversalTrigger] Player entered for the first time — waiting for exit.");
        }
        else if (!playerInside)
        {
            // Player has exited before and re-entered now → TRIGGER!
            playerInside = true;
            ActivateTrigger(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (triggerOnSecondPass && hasEnteredOnce)
        {
            playerInside = false;
            Debug.Log("[UniversalTrigger] Player exited — next entry will trigger.");
        }
    }

    private void ActivateTrigger(Collider other)
    {
        hasActivated = true;

        // Spawn the jumpscare object
        Vector3 position = spawnPoint ? spawnPoint.position : transform.position;
        GameObject spawned = Instantiate(targetPrefab, position, Quaternion.identity);

        // Trigger its jumpscare logic if available
        var scare = spawned.GetComponent<JumpscareController>();
        if (scare != null)
        {
            scare.BeginScare(other.gameObject, scareSound, scareDuration);
        }
        else
        {
            Debug.LogWarning($"[UniversalTrigger] Spawned object {spawned.name} has no JumpscareController!");
        }

        // Disable collider so it only triggers once
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Debug.Log("[UniversalTrigger] Trigger activated for " + other.name);
    }

}
