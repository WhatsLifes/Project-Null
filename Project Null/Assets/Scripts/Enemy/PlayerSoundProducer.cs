using UnityEngine;

public class PlayerSoundProducer : MonoBehaviour
{
    [Header("Sound Settings")]
    [Tooltip("Is the player currently making noise (can be heard by boss)?")]
    public bool isMakingNoise = true;

    [Header("Movement Detection")]
    [Tooltip("Speed threshold to determine if player is moving")]
    public float movementThreshold = 0.1f;

    [Header("Crouch Settings")]
    [Tooltip("Is the player currently crouched?")]
    public bool isCrouched = false;

    [Header("References")]
    private Rigidbody rb;
    private CharacterController characterController;

    [Header("Debug")]
    public bool showDebugInfo = true;

    private Vector3 lastPosition;
    private bool isMoving = false;

    private SimpleFPS fpsController;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
        fpsController = GetComponent<SimpleFPS>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        UpdateCrouchState();
        UpdateMovementState();
        UpdateNoiseState();
    }

    private void UpdateCrouchState()
    {
        // Auto-detect crouch
        if (fpsController != null)
        {
            // Check if controller height is closer to crouch height
            if (characterController != null)
            {
                isCrouched = characterController.height < 1.5f;
            }
        }
    }

    private void UpdateMovementState()
    {
        // Primary: Check CharacterController velocity
        if (characterController != null)
        {
            float speed = characterController.velocity.magnitude;
            isMoving = speed > movementThreshold;

            // Debug spam reduced - comment out if needed
            // if (showDebugInfo)
            // {
            //     Debug.Log($"PlayerSound: CC velocity = {speed:F3}, threshold = {movementThreshold}, isMoving = {isMoving}");
            // }
            return;
        }

        // Fallback: Check Rigidbody velocity
        if (rb != null && !rb.isKinematic)
        {
            float speed = rb.linearVelocity.magnitude;
            isMoving = speed > movementThreshold;

            if (showDebugInfo)
            {
                Debug.Log($"PlayerSound: RB velocity = {speed:F3}, threshold = {movementThreshold}, isMoving = {isMoving}");
            }
            return;
        }

        // Last resort: Check position change
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        float speedFromPosition = distanceMoved / Time.deltaTime;
        isMoving = speedFromPosition > movementThreshold;

        if (showDebugInfo)
        {
            Debug.Log($"PlayerSound: Position speed = {speedFromPosition:F3}, threshold = {movementThreshold}, isMoving = {isMoving}");
        }

        lastPosition = transform.position;
    }

    private void UpdateNoiseState()
    {
        // Player makes noise only when:
        // 1. They are moving
        // 2. They are NOT crouched
        isMakingNoise = isMoving && !isCrouched;
    }
    public void SetCrouched(bool crouched)
    {
        isCrouched = crouched;
        Debug.Log($"Player crouch state: {isCrouched}");
    }
    public bool IsMakingSound()
    {
        return isMakingNoise;
    }
    public bool IsMakingNoise()
    {
        return isMakingNoise;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
    public bool IsCrouched()
    {
        return isCrouched;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugInfo)
            return;

        // Sound indicator above player - RED = making noise, GREEN = silent
        Gizmos.color = isMakingNoise ? Color.red : Color.green;
        Vector3 indicatorPos = transform.position + Vector3.up * 3.5f;
        Gizmos.DrawWireSphere(indicatorPos, 0.5f);

        // Draw pulsing line when making noise
        if (isMakingNoise)
        {
            Gizmos.DrawLine(transform.position, indicatorPos);
            // Draw sound wave circles
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 1f, 2f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 1f, 4f);
        }

        // Movement indicator
        if (isMoving)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2.5f, Vector3.one * 0.3f);
        }

        // Crouch indicator
        if (isCrouched)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, new Vector3(1f, 0.2f, 1f));
        }
    }
}
