using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float bobFrequency = 6f;
    public float bobAmplitude = 0.05f;
    public float horizontalAmplitude = 0.03f;

    [Header("References")]
    public CharacterController controller;

    private float timer = 0f;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition;
    }

    void Update()
    {
        if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
        {
            timer += Time.deltaTime * bobFrequency;

            float bobOffsetY = Mathf.Sin(timer) * bobAmplitude;
            float bobOffsetX = Mathf.Cos(timer / 2f) * horizontalAmplitude;

            transform.localPosition = startPosition + new Vector3(bobOffsetX, bobOffsetY, 0f);
        }
        else
        {
            timer = 0f;

            // Smoothly return to original position
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                startPosition,
                Time.deltaTime * 5f
            );
        }
    }
}