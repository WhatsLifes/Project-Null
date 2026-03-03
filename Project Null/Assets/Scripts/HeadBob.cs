using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Header("Bobbing Settings")]
    [Tooltip("How fast the bob cycle runs (lower = heavier feel)")]
    public float bobFrequency = 4.5f;

    [Tooltip("Vertical head movement amount")]
    public float bobAmplitude = 0.022f;

    [Tooltip("Side-to-side movement amount")]
    public float horizontalAmplitude = 0.012f;

    [Tooltip("How quickly the camera moves toward its target position")]
    public float smoothSpeed = 10f;

    [Header("References")]
    public CharacterController controller;

    private float timer;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition;
    }

    void Update()
    {
        if (controller == null)
            return;

        float speed = controller.velocity.magnitude;
        float movementAmount = Mathf.Clamp01(speed);

        timer += Time.deltaTime * bobFrequency;

        float bobOffsetY = Mathf.Sin(timer) * bobAmplitude * movementAmount;
        float bobOffsetX = Mathf.Cos(timer * 0.5f) * horizontalAmplitude * movementAmount;

        Vector3 targetPosition = startPosition + new Vector3(bobOffsetX, bobOffsetY, 0f);

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * smoothSpeed
        );
    }
}