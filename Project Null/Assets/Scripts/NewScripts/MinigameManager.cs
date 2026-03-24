using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MinigameManager : MonoBehaviour
{
    [Header("Light Objects")]
    public Light redLight;
    public Light greenLight;

    [Header("Timing")]
    public float redLightDuration = 3f;
    public float greenLightDuration = 3f;

    [Header("References")]
    public PlankManager plankManager;

    private bool gameStarted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !gameStarted)
        {
            gameStarted = true;
            GetComponent<Collider>().enabled = false;
            StartCoroutine(LightCycle());
        }
    }

    private IEnumerator LightCycle()
    {
        while (true)
        {
            SetLight(isRed: true);
            plankManager.OnRedLight();
            yield return new WaitForSeconds(redLightDuration);

            SetLight(isRed: false);
            plankManager.OnGreenLight();
            yield return new WaitForSeconds(greenLightDuration);
        }
    }

    private void SetLight(bool isRed)
    {
        redLight.enabled = isRed;
        greenLight.enabled = !isRed;
    }
}