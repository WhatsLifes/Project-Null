using UnityEngine;

// This script handles showing and disappearing of any object
public class DisappearObject : MonoBehaviour
{
    [Header("Disappear Settings")]
    public float disappearTime = 1f; // Time in seconds before object vanishes

    private Renderer[] renderers;

    void Awake()
    {
        // Get all renderers in this prefab (including children)
        renderers = GetComponentsInChildren<Renderer>();

        // Hide all renderers initially
        foreach (var r in renderers)
        {
            r.enabled = false;
        }
    }

    // Call this function to show the object and make it disappear automatically
    public void TriggerDisappear()
    {
        // Show the object immediately
        foreach (var r in renderers)
        {
            r.enabled = true;
        }

        // Destroy the object after disappearTime seconds
        Destroy(gameObject, disappearTime);
    }
}
