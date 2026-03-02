using UnityEngine;

public class FlashlightBob : MonoBehaviour
{
    [Header("Movement Reference")]
    public CharacterController controller;

    [Header("Bobbing Settings")]
    public float bobFrequency = 6f;
    public float verticalAmplitude = 0.05f;
    public float horizontalAmplitude = 0.04f;
    public float tiltAmount = 2f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private float timer;

    void Start()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    void Update()
    {
        if (controller != null && controller.isGrounded && controller.velocity.magnitude > 0.1f)
        {
            timer += Time.deltaTime * bobFrequency;

            float offsetY = Mathf.Sin(timer) * verticalAmplitude;
            float offsetX = Mathf.Cos(timer * 0.5f) * horizontalAmplitude;
            float tilt = Mathf.Sin(timer) * tiltAmount;

            transform.localPosition = startPosition + new Vector3(offsetX, offsetY, 0f);
            transform.localRotation = startRotation * Quaternion.Euler(0f, 0f, tilt);
        }
        else
        {
            timer = 0f;

            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                startPosition,
                Time.deltaTime * 5f
            );

            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                startRotation,
                Time.deltaTime * 5f
            );
        }
    }
}