using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPS : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float mouseSensitivity = 2f;
    public Transform Camera;
    [HideInInspector] public float speedMultiplier = 1f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    public float crouchSpeed = 2.5f;
    public float crouchTransitionSpeed = 6f;

    [Header("Camera Acceleration Settings")]
    public bool enableCameraAcceleration = true;

    [Range(1f, 30f)]
    public float rotationAcceleration = 10f;

    [Range(0.001f, 0.5f)]
    public float smoothTime = 0.01f;

    private CharacterController controller;
    private float verticalLookRotation = 0f;
    private bool isCrouching = false;

    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool canLook = true;
    private bool isBeingForcedToLook = false;
    private Quaternion targetRotation;
    private float verticalVelocity = 0f;

    private Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentVelocity = Vector2.zero;
    private float targetVerticalRotation = 0f;
    private float currentVerticalRotation = 0f;
    private float targetHorizontalRotation = 0f;
    private float currentHorizontalRotation = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (FindObjectsOfType<SimpleFPS>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        targetRotation = transform.rotation;

        currentHorizontalRotation = transform.eulerAngles.y;
        targetHorizontalRotation = currentHorizontalRotation;
        currentVerticalRotation = verticalLookRotation;
        targetVerticalRotation = verticalLookRotation;

        StartCoroutine(ResetControllerAfterLoad());
    }

    private System.Collections.IEnumerator ResetControllerAfterLoad()
    {
        yield return null;

        verticalVelocity = 0f;
        currentMouseDelta = Vector2.zero;
        currentVelocity = Vector2.zero;

        controller.enabled = false;
        controller.enabled = true;
    }

    void OnEnable()
    {
        verticalVelocity = 0f;
    }

    void Update()
    {
        if (canMove)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 move = transform.right * h + transform.forward * v;

            if (move.magnitude > 0.1f)
                move = move.normalized;

            float baseSpeed = isCrouching ? crouchSpeed : speed;
            float currentSpeed = baseSpeed * speedMultiplier;

            Vector3 horizontalVel = move * currentSpeed;

            if (controller.isGrounded)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }

            Vector3 fullVelocity = horizontalVel;
            fullVelocity.y = verticalVelocity;
            controller.Move(fullVelocity * Time.deltaTime);
        }

        if (canLook)
        {
            if (enableCameraAcceleration)
                HandleSmoothLook();
            else
                HandleStandardLook();
        }
        else if (isBeingForcedToLook)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
            isCrouching = !isCrouching;

        float targetHeight = isCrouching ? crouchHeight : standHeight;
        controller.height = Mathf.MoveTowards(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        Vector3 camPos = Camera.localPosition;
        camPos.y = controller.height / 2f;
        Camera.localPosition = Vector3.MoveTowards(Camera.localPosition, camPos, Time.deltaTime * crouchTransitionSpeed);
    }

    void HandleSmoothLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        Vector2 targetMouseDelta = new Vector2(mouseX, mouseY) * mouseSensitivity;
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentVelocity, smoothTime);

        targetHorizontalRotation += currentMouseDelta.x;
        targetVerticalRotation -= currentMouseDelta.y;
        targetVerticalRotation = Mathf.Clamp(targetVerticalRotation, -90f, 90f);

        currentHorizontalRotation = Mathf.Lerp(currentHorizontalRotation, targetHorizontalRotation, Time.deltaTime * rotationAcceleration);
        currentVerticalRotation = Mathf.Lerp(currentVerticalRotation, targetVerticalRotation, Time.deltaTime * rotationAcceleration);

        transform.rotation = Quaternion.Euler(0f, currentHorizontalRotation, 0f);
        Camera.localEulerAngles = new Vector3(currentVerticalRotation, 0f, 0f);

        verticalLookRotation = currentVerticalRotation;
    }

    void HandleStandardLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        Camera.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);

        currentVerticalRotation = verticalLookRotation;
        targetVerticalRotation = verticalLookRotation;
        currentHorizontalRotation = transform.eulerAngles.y;
        targetHorizontalRotation = currentHorizontalRotation;
    }

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

        currentHorizontalRotation = transform.eulerAngles.y;
        targetHorizontalRotation = currentHorizontalRotation;
        currentVerticalRotation = verticalLookRotation;
        targetVerticalRotation = verticalLookRotation;
    }

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