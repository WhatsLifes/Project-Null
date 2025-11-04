using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;

public class ObjectiveTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HUD hud;

    [Header("Settings")]
    [SerializeField] private int objectiveNumber = 3;
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player entering
        if (other.CompareTag("Player"))
        {
            // If set to trigger once and already triggered, exit
            if (triggerOnce && hasTriggered)
                return;

            // Show the appropriate objective
            switch (objectiveNumber)
            {
                case 1:
                    hud.ShowObjective1();
                    break;
                case 2:
                    hud.ShowObjective2();
                    break;
                case 3:
                    hud.ShowObjective3();
                    break;
                default:
                    Debug.LogWarning($"Invalid objective number: {objectiveNumber}");
                    break;
            }

            hasTriggered = true;

            Debug.Log($"Objective {objectiveNumber} triggered!");
        }
    }
}
