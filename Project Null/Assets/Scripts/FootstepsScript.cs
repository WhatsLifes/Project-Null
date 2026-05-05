using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource footstepAudioSource;
    public AudioSource grassFootstepAudioSource;

    [Header("Movement Settings")]
    [Tooltip("How fast must the player move to trigger sound?")]
    public float velocityThreshold = 0.1f;
    public float raycastDistance = 1.2f;

    private Vector3 lastPosition;
    private float currentVelocity;
    private AudioSource currentActiveAudioSource;

    void Start()
    {
        // start muted until we move
        footstepAudioSource.mute = true;
        grassFootstepAudioSource.mute = true;
        currentActiveAudioSource = footstepAudioSource;
        
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
            PlayFootsteps();
        }
        else
        {
            footstepAudioSource.mute = true;
            grassFootstepAudioSource.mute = true;
        }

        lastPosition = transform.position;
    }

    void PlayFootsteps()
    {
        // default to main footstep sounds
        AudioSource sourceToPlay = footstepAudioSource;

        // check floor - raycast down from current position over raycastDistance and store the object that is hit into hit variable
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance))
        {
            if (hit.collider.CompareTag("grass"))
            {
                Debug.Log("Grass detected.");
                sourceToPlay = grassFootstepAudioSource;
            }
        }

        // compare current to new surface
        if (currentActiveAudioSource != sourceToPlay)
        {
            currentActiveAudioSource.mute = true;
            currentActiveAudioSource = sourceToPlay;
        }

        // play footstep sounds
        if (sourceToPlay.mute) sourceToPlay.mute = false;
    }
}