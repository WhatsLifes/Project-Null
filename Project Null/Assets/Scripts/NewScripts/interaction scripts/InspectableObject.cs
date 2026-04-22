using UnityEngine;

[DefaultExecutionOrder(100)]
public class InspectableObject : MonoBehaviour, InteractableScript
{
    [Header("Camera Reference")]
    [Tooltip("Drag your Camera GameObject here. If left empty the script will find it automatically.")]
    [SerializeField] private Transform cameraOverride;

    [Header("Hold Settings")]
    [SerializeField] private Vector3 holdPositionOffset = new Vector3(0, -0.6f, 0.7f);

    [Header("Inspect Settings")]
    [SerializeField] private Vector3 inspectPositionOffset = new Vector3(0, 0, 1.5f);
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float scrollSpeed = 1f;
    [SerializeField] private float scrollMin = -0.5f;
    [SerializeField] private float scrollMax = 0.5f;

    [Header("Inspect UI (optional)")]
    [SerializeField] private Canvas inspectPromptCanvas;

    private bool isBeingHeld = false;
    private bool isInspecting = false;
    private bool justPickedUp = false;
    private Quaternion inspectRotation;
    private Quaternion holdRotationOffset = Quaternion.identity;
    private float currentScrollOffset = 0f;

    private Transform cameraTransform;
    private SimpleFPS playerController;

    private Vector3 originalWorldPosition;
    private Quaternion originalWorldRotation;

    private Vector3 frozenCamPos;
    private Vector3 frozenCamForward;
    private Vector3 frozenCamUp;
    private Vector3 frozenCamRight;

    private void Start()
    {
        ResolveCamera();
    }

    private void ResolveCamera()
    {
        // Always try to find the player controller so canLook/canMove can be toggled
        playerController = FindObjectOfType<SimpleFPS>();

        if (cameraOverride != null)
        {
            cameraTransform = cameraOverride;
            return;
        }

        if (playerController != null && playerController.Camera != null)
        {
            cameraTransform = playerController.Camera;
            return;
        }

        Camera found = Camera.main ?? FindObjectOfType<Camera>();
        if (found != null)
            cameraTransform = found.transform;

        if (cameraTransform == null)
            Debug.LogError("InspectableObject: Could not find a camera — drag one into Camera Override in the Inspector.");
    }

    public void InteractScript()
    {
        if (PlayerHold.Instance == null) { Debug.LogError("InspectableObject: PlayerHold.Instance is null — add PlayerHold to the Player GameObject."); return; }
        if (PlayerHold.Instance.IsHoldingObject()) return;

        if (cameraTransform == null) ResolveCamera();
        if (cameraTransform == null) return;

        originalWorldPosition = transform.position;
        originalWorldRotation = transform.rotation;

        holdRotationOffset = Quaternion.identity;
        isBeingHeld = true;
        justPickedUp = true;
        PlayerHold.Instance.Pickup(gameObject);
    }

    private void Update()
    {
        if (isBeingHeld && (PlayerHold.Instance == null || PlayerHold.Instance.heldObject != gameObject))
        {
            ResetHeldState();
            return;
        }

        if (!isBeingHeld) return;

        if (justPickedUp) { justPickedUp = false; return; }

        if (isInspecting)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            if (mouseX != 0 || mouseY != 0)
            {
                // Rotate the object independently of the camera
                inspectRotation = Quaternion.AngleAxis(-mouseX * rotationSpeed * Time.deltaTime, Vector3.up) * inspectRotation;
                inspectRotation = Quaternion.AngleAxis(mouseY * rotationSpeed * Time.deltaTime, Vector3.right) * inspectRotation;
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
                inspectPositionOffset.z = Mathf.Clamp(inspectPositionOffset.z - scroll * scrollSpeed, scrollMin, scrollMax); // Adjust zoom by modifying z-offset

            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Escape))
                ExitInspect();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.R))
                EnterInspect();

            if (Input.GetKeyDown(KeyCode.E))
                PlayerHold.Instance.Drop();
        }
    }

    private void LateUpdate()
    {
        if (!isBeingHeld || cameraTransform == null) return;

        if (isInspecting)
        {
            transform.position =
                  frozenCamPos
                + frozenCamForward * inspectPositionOffset.z
                + frozenCamUp * inspectPositionOffset.y
                + frozenCamRight * inspectPositionOffset.x;

            transform.rotation = inspectRotation;
        }
        else
        {
            // Position the object relative to the camera when holding, but dont move camera
            transform.position =
                  cameraTransform.position
                + cameraTransform.forward * holdPositionOffset.z
                + cameraTransform.up * holdPositionOffset.y
                + cameraTransform.right * holdPositionOffset.x;

            transform.rotation = cameraTransform.rotation * holdRotationOffset;
        }
    }

    private void EnterInspect()
    {
        isInspecting = true;
        inspectRotation = transform.rotation;
        currentScrollOffset = 0f;

        // Snapshot the camera's world basis so it stays locked for the duration of inspect
        frozenCamPos = cameraTransform.position;
        frozenCamForward = cameraTransform.forward;
        frozenCamUp = cameraTransform.up;
        frozenCamRight = cameraTransform.right;

        if (playerController != null)
        {
            playerController.canLook = false;
            playerController.canMove = false;
        }

        if (inspectPromptCanvas != null)
            inspectPromptCanvas.gameObject.SetActive(true);
    }

    private void ExitInspect()
    {
        isInspecting = false;
        currentScrollOffset = 0f;
        holdRotationOffset = Quaternion.Inverse(cameraTransform.rotation) * inspectRotation;

        if (playerController != null)
        {
            playerController.canLook = true;
            playerController.canMove = true;
        }

        if (inspectPromptCanvas != null)
            inspectPromptCanvas.gameObject.SetActive(false);
    }

    private void ResetHeldState()
    {
        if (isInspecting) ExitInspect();
        isBeingHeld = false;

        // Removed resetting to original position and rotation to retain final orientation
        // transform.position = originalWorldPosition;
        // transform.rotation = originalWorldRotation;
    }
}
