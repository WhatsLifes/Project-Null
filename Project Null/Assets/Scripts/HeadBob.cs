using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Header("Bobbing Settings")]
    [Tooltip("How fast the bob cycle runs (lower = heavier feel)")]
    public float bobFrequency = 5f;

    [Tooltip("Vertical tilt amount in degrees")]
    public float bobAmplitude = 2f;

    [Tooltip("Side-to-side tilt amount in degrees")]
    public float horizontalAmplitude = 1.2f;

    [Tooltip("How quickly the bob interpolates")]
    public float smoothSpeed = 8f;

    [Tooltip("Minimum speed to trigger bobbing (prevents micro-jitter)")]
    public float movementThreshold = 0.1f;

    [Header("References")]
    public CharacterController controller;

    private float timer;
    private float currentTiltX;
    private float currentTiltZ;

    void LateUpdate()
    {
        if (controller == null || !controller.enabled)
        {
            timer = 0f;
            currentTiltX = Mathf.Lerp(currentTiltX, 0f, Time.deltaTime * smoothSpeed);
            currentTiltZ = Mathf.Lerp(currentTiltZ, 0f, Time.deltaTime * smoothSpeed);
            ApplyBob();
            return;
        }

        float speed = controller.velocity.magnitude;

        if (speed < movementThreshold)
        {
            timer = 0f;
            currentTiltX = Mathf.Lerp(currentTiltX, 0f, Time.deltaTime * smoothSpeed);
            currentTiltZ = Mathf.Lerp(currentTiltZ, 0f, Time.deltaTime * smoothSpeed);
            ApplyBob();
            return;
        }

        float movementAmount = Mathf.Clamp01(speed);

        timer += Time.deltaTime * bobFrequency;

        float targetTiltX = Mathf.Sin(timer) * bobAmplitude * movementAmount;
        float targetTiltZ = Mathf.Cos(timer * 0.5f) * horizontalAmplitude * movementAmount;

        currentTiltX = Mathf.Lerp(currentTiltX, targetTiltX, Time.deltaTime * smoothSpeed);
        currentTiltZ = Mathf.Lerp(currentTiltZ, targetTiltZ, Time.deltaTime * smoothSpeed);

        ApplyBob();
    }

    private void ApplyBob()
    {
        // Layer the bob ON TOP of whatever rotation the mouse look already set this frame
        transform.localRotation *= Quaternion.Euler(currentTiltX, 0f, currentTiltZ);
    }
}