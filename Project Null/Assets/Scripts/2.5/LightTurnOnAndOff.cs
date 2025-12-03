using UnityEngine;
using System.Collections;

public class LightTurnOnAndOff : MonoBehaviour
{
[Header("Lights to TURN ON (optional, max 3)")]
public Light lightToTurnOn1;
public Light lightToTurnOn2;
public Light lightToTurnOn3;

[Header("Lights to TURN OFF (optional, max 3)")]
public Light lightToTurnOff1;
public Light lightToTurnOff2;
public Light lightToTurnOff3;

[Header("Settings")]
[SerializeField] private float delayBeforeSwitch = 1f; // set in inspector

private bool hasSwitched = false;

// Called by PickupFlower script
public void SwitchLights()
{
    if (hasSwitched) return; // Prevent double activation
    StartCoroutine(SwitchRoutine());
}

private IEnumerator SwitchRoutine()
{
    yield return new WaitForSeconds(delayBeforeSwitch);

    // Turn ON lights if assigned
    if (lightToTurnOn1 != null) lightToTurnOn1.enabled = true;
    if (lightToTurnOn2 != null) lightToTurnOn2.enabled = true;
    if (lightToTurnOn3 != null) lightToTurnOn3.enabled = true;

    // Turn OFF lights if assigned
    if (lightToTurnOff1 != null) lightToTurnOff1.enabled = false;
    if (lightToTurnOff2 != null) lightToTurnOff2.enabled = false;
    if (lightToTurnOff3 != null) lightToTurnOff3.enabled = false;

    hasSwitched = true;

    Debug.Log($"LightTurnOnAndOff: Switched lights after {delayBeforeSwitch} seconds.");
}

}
