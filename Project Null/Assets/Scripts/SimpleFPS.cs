using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPS : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float mouseSensitivity = 2f;
    public Transform Camera;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    public float crouchSpeed = 2.5f;
    public float crouchTransitionSpeed = 6f;

    [Header("Camera Acceleration Settings")]
    [Tooltip("Enable smooth camera acceleration")]
    public bool enableCameraAcceleration = true;

    [Tooltip("How quickly camera accelerates to target rotation")]
    [Range(1f, 30f)]
    public float rotationAcceleration = 10f;

    [Tooltip("Smoothing factor - lower = smoother but more lag")]
    [Range(0.001f, 0.5f)]
    public float smoothTime = 0.01f;

    private CharacterController controller;
    private float verticalLookRotation = 0f;
    private bool isCrouching = false;

    // ✅ Added for jumpscare control
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool canLook = true;
    private bool isBeingForcedToLook = false;
    private Quaternion targetRotation;
    private float verticalVelocity = 0f;

    // Camera acceleration variables
    private Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentVelocity = Vector2.zero;
    private float targetVerticalRotation = 0f;
    private float currentVerticalRotation = 0f;
    private float targetHorizontalRotation = 0f;
    private float currentHorizontalRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        targetRotation = transform.rotation;

        // Initialize rotation values
        currentHorizontalRotation = transform.eulerAngles.y;
        targetHorizontalRotation = currentHorizontalRotation;
        currentVerticalRotation = verticalLookRotation;
        targetVerticalRotation = verticalLookRotation;
    }

    void Update()
    {
        // --- Movement ---
        if (canMove)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 move = transform.right * h + transform.forward * v;

            if (move.magnitude > 0.1f)
                move = move.normalized;

            float currentSpeed = isCrouching ? crouchSpeed : speed;

            // HORIZONTAL ONLY (XZ)
            Vector3 horizontalVel = move * currentSpeed;

            // GRAVITY (Y)
            if (controller.isGrounded)
            {
                verticalVelocity = -2f;   // keep grounded
            }
            else
            {
                verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }

            // COMBINE ALL VELOCITY
            Vector3 fullVelocity = horizontalVel;
            fullVelocity.y = verticalVelocity;
            controller.Move(fullVelocity * Time.deltaTime);
        }

        // --- Look ---
        if (canLook)
        {
            if (enableCameraAcceleration)
            {
                HandleSmoothLook();
            }
            else
            {
                HandleStandardLook();
            }
        }
        else if (isBeingForcedToLook)
        {
            // Smooth rotation toward target
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        // --- Crouching (Toggle) ---
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching; // Toggle crouch on/off
        }

        // Smoothly transition height
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        controller.height = Mathf.MoveTowards(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        // Smoothly move camera
        Vector3 camPos = Camera.localPosition;
        camPos.y = controller.height / 2f;
        Camera.localPosition = Vector3.MoveTowards(Camera.localPosition, camPos, Time.deltaTime * crouchTransitionSpeed);
    }

    void HandleSmoothLook()
    {
        // Get raw mouse input
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        // Smooth the mouse delta using SmoothDamp for acceleration feel
        Vector2 targetMouseDelta = new Vector2(mouseX, mouseY) * mouseSensitivity;
        currentMouseDelta = Vector2.SmoothDamp(
            currentMouseDelta,
            targetMouseDelta,
            ref currentVelocity,
            smoothTime
        );

        // Update target rotations
        targetHorizontalRotation += currentMouseDelta.x;
        targetVerticalRotation -= currentMouseDelta.y;
        targetVerticalRotation = Mathf.Clamp(targetVerticalRotation, -90f, 90f);

        // Smoothly interpolate current rotations to target
        currentHorizontalRotation = Mathf.Lerp(
            currentHorizontalRotation,
            targetHorizontalRotation,
            Time.deltaTime * rotationAcceleration
        );

        currentVerticalRotation = Mathf.Lerp(
            currentVerticalRotation,
            targetVerticalRotation,
            Time.deltaTime * rotationAcceleration
        );

        // Apply rotations
        transform.rotation = Quaternion.Euler(0f, currentHorizontalRotation, 0f);
        Camera.localEulerAngles = new Vector3(currentVerticalRotation, 0f, 0f);

        // Update the stored vertical rotation for compatibility
        verticalLookRotation = currentVerticalRotation;
    }

    void HandleStandardLook()
    {
        // Original look code (no acceleration)
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        Camera.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);

        // Sync with smooth look variables
        currentVerticalRotation = verticalLookRotation;
        targetVerticalRotation = verticalLookRotation;
        currentHorizontalRotation = transform.eulerAngles.y;
        targetHorizontalRotation = currentHorizontalRotation;
    }

    // ✅ New methods used by JumpscareController
    public void ForceLookAt(Vector3 position)
    {
        canLook = false;
        canMove = false;
        isBeingForcedToLook = true;
        Vector3 dir = (position - Camera.position).normalized;
        dir.y = 0f;
        targetRotation = Quaternion.LookRotation(dir);
    }

    public void ReleaseLook()
    {
        isBeingForcedToLook = false;
        canLook = true;
        canMove = true;

        // Sync rotation values when releasing control
        currentHorizontalRotation = transform.eulerAngles.y;
        targetHorizontalRotation = currentHorizontalRotation;
        currentVerticalRotation = verticalLookRotation;
        targetVerticalRotation = verticalLookRotation;
    }

    /// <summary>
    /// Add camera shake effect (useful for jump scares, impacts, etc.)
    /// </summary>
    public void ShakeCamera(float duration, float magnitude)
    {
        StartCoroutine(CameraShakeCoroutine(duration, magnitude));
    }

    private System.Collections.IEnumerator CameraShakeCoroutine(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.localPosition = originalPos;
    }
}