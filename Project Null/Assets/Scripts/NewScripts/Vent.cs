using UnityEngine;

public class VentBreak : MonoBehaviour
{
    [Header("Vent Settings")]
    public int health = 50;                         // How much damage it takes before breaking
    public GameObject brokenVentPrefab;              // Optional broken version prefab
    public AudioClip breakSound;                     // Optional sound effect
    public float destroyDelay = 2f;                  // Delay before removing the broken vent

    private bool isBroken = false;

    // Called when the vent takes damage
    public void TakeDamage(int damage)
    {
        if (isBroken) return;

        health -= damage;

        if (health <= 0)
        {
            BreakVent();
        }
    }

    void BreakVent()
    {
        isBroken = true;

        // Play break sound if assigned
        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(breakSound, transform.position);
        }

        // Spawn broken version (optional)
        if (brokenVentPrefab != null)
        {
            Instantiate(brokenVentPrefab, transform.position, transform.rotation);
        }

        // Disable collider so player can pass through
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // Disable mesh renderer so it disappears visually
        MeshRenderer rend = GetComponent<MeshRenderer>();
        if (rend != null)
            rend.enabled = false;

        // Optionally destroy object after delay
        Destroy(gameObject, destroyDelay);
    }
}
