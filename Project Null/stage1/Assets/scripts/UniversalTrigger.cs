using UnityEngine;

// This script triggers any prefab to appear/disappear when the player enters the trigger
//Box trigger or capsule triggers maybe even ray lines
public class UniversalTrigger : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject TargetPrefab; // Drag any prefab here (not just ghosts)

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger when the player enters
        if (other.CompareTag("Player"))
        {
            // Spawn the prefab at the trigger's position
            GameObject spawnedObject = Instantiate(TargetPrefab, transform.position, Quaternion.identity);

            // Attempt to find a script that controls disappearing
            var disappearScript = spawnedObject.GetComponent<DisappearObject>();
            if (disappearScript != null)
            {
                disappearScript.TriggerDisappear();
            }

            // Optional: Add support for other scripts in the future
        }
    }
}
