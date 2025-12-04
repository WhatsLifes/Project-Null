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

    private CharacterController controller;
    private float verticalLookRotation = 0f;
    private bool isCrouching = false;

    // ✅ Added for jumpscare control
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool canLook = true;
    private bool isBeingForcedToLook = false;
    private Quaternion targetRotation;
    private float verticalVelocity = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        targetRotation = transform.rotation;
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
            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);
            verticalLookRotation -= mouseY;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
            Camera.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
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
    }
}