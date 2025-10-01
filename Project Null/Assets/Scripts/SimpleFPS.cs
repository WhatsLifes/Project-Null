using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPS : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;                 // Normal walking speed
    public float mouseSensitivity = 2f;      // How sensitive the mouse look is
    public Transform Camera;                 // Reference to the player's camera (usually a child object)

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;          // Player height when crouching
    public float standHeight = 2f;           // Normal standing height
    public float crouchSpeed = 2.5f;         // Slower movement while crouched
    public float crouchTransitionSpeed = 6f; // How quickly we transition between crouch/stand

    private CharacterController controller;
    private float verticalLookRotation = 0f; // Tracks how far we’ve looked up/down
    private bool isCrouching = false;        // Whether the player is currently crouched

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Lock the cursor to the center of the screen so the mouse only controls camera
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- Movement ---
        float h = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow keys
        float v = Input.GetAxis("Vertical");   // W/S or Up/Down arrow keys

        // Calculate movement direction relative to where player is facing
        Vector3 move = transform.right * h + transform.forward * v;

        // Use crouch speed if crouching, otherwise normal walking speed
        float currentSpeed = isCrouching ? crouchSpeed : speed;

        // Actually move the character controller
        controller.SimpleMove(move * currentSpeed);

        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity; // Left/right look
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity; // Up/down look

        // Rotate player left/right
        transform.Rotate(Vector3.up * mouseX);

        // Clamp vertical camera rotation so you can’t break your neck
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        // Apply vertical look rotation to the camera only
        Camera.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);

        // Holding Left Ctrl makes you crouch
        if (Input.GetKey(KeyCode.LeftControl))
        {
            isCrouching = true;
        }
        else
        {
            // Only stand up if we’re crouched AND there’s space above us
            if (isCrouching && CanStandUp())
                isCrouching = false;
        }

        
        // Smoothly lerp player height between crouch/stand
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        controller.height = Mathf.MoveTowards(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        // Keep camera at half the controller’s height so it looks natural
        Vector3 camPos = Camera.localPosition;
        camPos.y = controller.height / 2f;
        Camera.localPosition = Vector3.MoveTowards(Camera.localPosition, camPos, Time.deltaTime * crouchTransitionSpeed);
    }

    //Check if there's enough space above the player to stand up
    private bool CanStandUp()
    {
        Vector3 basePos = controller.transform.position;

        // Height when crouched vs. when standing
        float crouchTop = basePos.y + crouchHeight;
        float standTop = basePos.y + standHeight;

        // Define the capsule we want to test: from current crouch top up to full standing top
        Vector3 start = new Vector3(basePos.x, crouchTop, basePos.z);
        Vector3 end = new Vector3(basePos.x, standTop, basePos.z);

        // Slightly shrink radius so we don’t get false positives near walls
        float radius = controller.radius * 0.9f;

        // If Physics finds nothing in the way → it’s safe to stand
        return !Physics.CheckCapsule(start, end, radius, LayerMask.GetMask("Default"));
    }
}
