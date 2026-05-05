using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource footstepAudioSource;

    [Header("Movement Settings")]
    [Tooltip("How fast must the player move to trigger sound?")]
    public float velocityThreshold = 0.1f;

    private Vector3 lastPosition;
    private float currentVelocity;

    void Start()
    {
        // start muted until we move
        footstepAudioSource.mute = true;
        
        // initialize previous position
        lastPosition = transform.position;
    }

    void Update()
    {
        // calculate velocity
        currentVelocity = (transform.position - lastPosition).magnitude / Time.deltaTime;

        // check velocity against threshold - move if above threshold
        if (currentVelocity > velocityThreshold)
        {
            if (footstepAudioSource.mute) footstepAudioSource.mute = false;
        }
        else
        {
            if (!footstepAudioSource.mute) footstepAudioSource.mute = true;
        }

        lastPosition = transform.position;
    }
}