using UnityEngine;

public class DisappearObject : MonoBehaviour
{
    [Header("Disappear Settings")]
    public float disappearTime = 1f;

    private Renderer[] renderers;
    private bool hasTriggered = false;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        foreach (var r in renderers)
        {
            r.enabled = false;
        }
    }

    public void TriggerDisappear()
    {
        if (hasTriggered) return; // prevent multiple activations
        hasTriggered = true;

        foreach (var r in renderers)
        {
            r.enabled = true;
        }

        Destroy(gameObject, disappearTime);
    }
}
