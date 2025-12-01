using UnityEngine;
using System.Collections;

public class ActivatePointLight : MonoBehaviour
{
    public Light flickerLight;
    public KeyCode toggleKey = KeyCode.L;

    public float minTime = 0.05f;
    public float maxTime = 0.2f;
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;

    private bool isOn = false;   // starts OFF

    void Start()
    {
        if (flickerLight == null)
            flickerLight = GetComponent<Light>();

        // Start OFF
        flickerLight.intensity = 0f;

        StartCoroutine(FlickerRoutine());
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isOn = !isOn;

            if (!isOn)
                flickerLight.intensity = 0f;  // force OFF intensity
        }
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            if (isOn)
            {
                flickerLight.intensity = Random.Range(minIntensity, maxIntensity);
            }
            else
            {
                flickerLight.intensity = 0f;
            }

            yield return new WaitForSeconds(Random.Range(minTime, maxTime));
        }
    }
}
