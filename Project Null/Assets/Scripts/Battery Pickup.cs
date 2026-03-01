using UnityEngine;

public class BatteryPickup : MonoBehaviour, InteractableScript
{
    [Header("Battery Settings")]
    [Tooltip("Amount of battery this pickup restores")]
    [Range(1f, 100f)]
    public float batteryRechargeAmount = 25f;

    [Header("References")]
    [Tooltip("The FlashlightToggle script to recharge")]
    public FlashlightToggle flashlightToggleScript;

    [Header("Pickup Settings")]
    [Tooltip("Maximum distance player can be to pick up")]
    public float pickupDistance = 3f;

    [Tooltip("Tag of the player (to detect proximity)")]
    public string playerTag = "Player";

    [Header("Visual Feedback (Optional)")]
    [Tooltip("Optional: Particle effect to play on pickup")]
    public ParticleSystem pickupEffect;

    [Tooltip("Optional: Sound to play on pickup")]
    public AudioClip pickupSound;

    private Transform player;
    private AudioSource audioSource;

    void Start()
    {
        // Find the player in the scene
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("BatteryPickup: No object with tag '" + playerTag + "' found!");
        }

        // Setup audio source if needed
        if (pickupSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    public void InteractScript()
    {
        PickupBattery();
    }

    void PickupBattery()
    {
        // Recharge the flashlight battery
        if (flashlightToggleScript != null)
        {
            bool success = flashlightToggleScript.RechargeBattery(batteryRechargeAmount);

            if (success)
            {
                // Play visual effect
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                // Play sound
                if (pickupSound != null && audioSource != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // Notify the pickup manager if it exists
                StartingRoomPickupManager.Instance?.ItemPickedUp(gameObject);

                // Make this object disappear
                gameObject.SetActive(false);

                Debug.Log($"Battery picked up! Recharged by {batteryRechargeAmount}%");
            }
            else
            {
                Debug.Log("Battery is already full - cannot pick up battery pack!");
            }
        }
        else
        {
            Debug.LogWarning("BatteryPickup: FlashlightToggle reference is missing!");
        }
    }

    // Optional: Draw the pickup radius in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}