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

    [Header("Prevent passing through dolls (optional)")]
    public LayerMask repelLayer;
    public float repelStrength = 0.1f;

    private CharacterController controller;
    private float verticalLookRotation = 0f;
    private bool isCrouching = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleCrouch();
        PreventPassThroughDolls();
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;
        float currentSpeed = isCrouching ? crouchSpeed : speed;
        controller.SimpleMove(move * currentSpeed);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        Camera.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
    }

    void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.LeftControl))
            isCrouching = true;
        else if (isCrouching && CanStandUp())
            isCrouching = false;

        float targetHeight = isCrouching ? crouchHeight : standHeight;
        controller.height = Mathf.MoveTowards(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        Vector3 camPos = Camera.localPosition;
        camPos.y = controller.height / 2f;
        Camera.localPosition = Vector3.MoveTowards(Camera.localPosition, camPos, Time.deltaTime * crouchTransitionSpeed);
    }

    bool CanStandUp()
    {
        Vector3 basePos = controller.transform.position;
        float crouchTop = basePos.y + crouchHeight;
        float standTop = basePos.y + standHeight;

        Vector3 start = new Vector3(basePos.x, crouchTop, basePos.z);
        Vector3 end = new Vector3(basePos.x, standTop, basePos.z);
        float radius = controller.radius * 0.9f;

        return !Physics.CheckCapsule(start, end, radius, LayerMask.GetMask("Default"));
    }

    void PreventPassThroughDolls()
    {
        if (repelLayer == 0) return;

        Vector3 bottom = transform.position;
        Vector3 top = bottom + Vector3.up * controller.height;
        Collider[] hits = Physics.OverlapCapsule(bottom, top, controller.radius, repelLayer);

        foreach (var col in hits)
        {
            Vector3 closest = col.ClosestPoint(transform.position);
            Vector3 pushDir = (transform.position - closest);
            if (pushDir.sqrMagnitude < 0.0001f) continue;
            pushDir = pushDir.normalized;
            controller.Move(pushDir * repelStrength);
        }
    }

    void OnGUI()
    {
        float size = 6f;
        float x = (Screen.width - size) / 2;
        float y = (Screen.height - size) / 2;
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(x, y, size, size), Texture2D.whiteTexture);
    }
}
