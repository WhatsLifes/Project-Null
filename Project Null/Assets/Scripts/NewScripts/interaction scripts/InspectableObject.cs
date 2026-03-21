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
    private float currentScrollOffset = 0f;

    private Transform cameraTransform;
    private SimpleFPS playerController;

    private Vector3 originalWorldPosition;
    private Quaternion originalWorldRotation;

    private void Start()
    {
        ResolveCamera();
    }

    private void ResolveCamera()
    {
        if (cameraOverride != null)
        {
            cameraTransform = cameraOverride;
            return;
        }

        playerController = FindObjectOfType<SimpleFPS>();
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
        if (PlayerHold.Instance == null || PlayerHold.Instance.IsHoldingObject()) return;

        if (cameraTransform == null) ResolveCamera();
        if (cameraTransform == null) return;

        originalWorldPosition = transform.position;
        originalWorldRotation = transform.rotation;

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
                inspectRotation = Quaternion.AngleAxis(-mouseX * rotationSpeed * Time.deltaTime, Vector3.up) * inspectRotation;
                inspectRotation = Quaternion.AngleAxis(mouseY * rotationSpeed * Time.deltaTime, cameraTransform.right) * inspectRotation;
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
                currentScrollOffset = Mathf.Clamp(currentScrollOffset + scroll * scrollSpeed, scrollMin, scrollMax);

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
                  cameraTransform.position
                + cameraTransform.forward * inspectPositionOffset.z
                + cameraTransform.up * (inspectPositionOffset.y + currentScrollOffset)
                + cameraTransform.right * inspectPositionOffset.x;

            transform.rotation = inspectRotation;
        }
        else
        {
            transform.position =
                  cameraTransform.position
                + cameraTransform.forward * holdPositionOffset.z
                + cameraTransform.up * holdPositionOffset.y
                + cameraTransform.right * holdPositionOffset.x;

            transform.rotation = cameraTransform.rotation;
        }
    }

    private void EnterInspect()
    {
        isInspecting = true;
        inspectRotation = transform.rotation;
        currentScrollOffset = 0f;

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

        transform.position = originalWorldPosition;
        transform.rotation = originalWorldRotation;
    }
}
