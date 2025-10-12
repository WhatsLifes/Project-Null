using UnityEngine;

public class CameraLightFlicker : MonoBehaviour
{
    public Light redLight;
    public float flickInterval = 1f;
    public float onIntensity = 20f;
    public float offIntensity = 0f;

    private float timer = 0f;
    private bool isOn = true;

    void Start()
    {
        if (redLight == null)
            redLight = GetComponent<Light>();
        redLight.intensity = onIntensity;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= flickInterval)
        {
            isOn = !isOn;
            redLight.intensity = isOn ? onIntensity : offIntensity;
            timer = 0f;
        }
    }
}
