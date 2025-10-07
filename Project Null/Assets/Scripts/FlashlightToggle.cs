using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    public Light flashlight;  // Assign in Inspector
    public KeyCode toggleKey = KeyCode.F;
    private bool isOn = false;

    void Start()
    {
        if (flashlight != null)
            flashlight.enabled = false;  // start off
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isOn = !isOn;
            flashlight.enabled = isOn;
        }
    }
}