using UnityEngine;

public class UniversalTrigger : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject TargetPrefab;

    private bool hasActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasActivated) return; // only activate once

        if (other.CompareTag("Player"))
        {
            hasActivated = true; // mark as used

            GameObject spawnedObject = Instantiate(TargetPrefab, transform.position, Quaternion.identity);

            var disappearScript = spawnedObject.GetComponent<DisappearObject>();
            if (disappearScript != null)
            {
                disappearScript.TriggerDisappear();
            }

            // Disable collider so it can’t trigger again
            GetComponent<Collider>().enabled = false;
        }
    }
}
