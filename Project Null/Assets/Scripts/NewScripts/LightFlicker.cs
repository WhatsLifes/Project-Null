using UnityEngine;
using System.Collections;

public class LightFlicker : MonoBehaviour
{
    public Light flickerLight;       // Assign your light in the inspector
    public float minTime = 0.05f;    // Minimum time between flickers
    public float maxTime = 0.2f;     // Maximum time between flickers
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;

    private void Start()
    {
        if (flickerLight == null)
            flickerLight = GetComponent<Light>();

        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            flickerLight.intensity = Random.Range(minIntensity, maxIntensity);
            flickerLight.enabled = (Random.value > 0.1f); // 10% chance to turn off briefly
            yield return new WaitForSeconds(Random.Range(minTime, maxTime));
        }
    }
}