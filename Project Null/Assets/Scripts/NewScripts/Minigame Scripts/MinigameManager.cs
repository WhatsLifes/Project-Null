using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MinigameManager : MonoBehaviour
{
    [Header("Light Objects")]
    public Light[] lights;

    [Header("Timing")]
    public float redLightDuration = 3f;
    public float greenLightDuration = 3f;

    [Header("References")]
    public PlankManager plankManager;
    public MinigameExit minigameExit;

    public bool gameStarted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !gameStarted)
        {
            gameStarted = true;
            minigameExit.gameObject.SetActive(true);
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
        foreach (Light l in lights)
        {
            l.color = isRed ? Color.red : Color.green;
        }
    }
}