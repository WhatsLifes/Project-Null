using sc.terrain.proceduralpainter;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Collections.AllocatorManager;
using static UnityEngine.InputManagerEntry;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class ShowObjectiveTrigger : MonoBehaviour
{
    [Header("HUD Reference")]
    [SerializeField] private HUD hud;

    [Header("Objective Settings")]
    [SerializeField] private string newObjectiveText = "New Objective";
    [SerializeField] private bool usePresetObjective = false;
    [SerializeField] private int presetObjectiveNumber = 1; // 1-8 for ShowObjective1() through ShowObjective8()

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool disableAfterTrigger = true;

    [Header("Optional")]
    [SerializeField] private DialogueTrigger dialogueTrigger; // Optional dialogue when objective appears

    private bool hasTriggered = false;

    private void Start()
    {
        // Auto-find HUD if not assigned
        if (hud == null)
        {
            hud = FindObjectOfType<HUD>();
            if (hud == null)
            {
                Debug.LogError("ObjectiveTrigger: HUD not found!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if player entered
        if (!other.CompareTag("Player"))
            return;

        // Check if already triggered
        if (triggerOnce && hasTriggered)
            return;

        // Trigger the objective
        TriggerObjective();
    }

    private void TriggerObjective()
    {
        hasTriggered = true;

        if (hud == null)
        {
            Debug.LogError("ObjectiveTrigger: HUD is null!");
            return;
        }

        // Show objective
        if (usePresetObjective)
        {
            ShowPresetObjective();
        }
        else
        {
            hud.ShowObjective(newObjectiveText);
        }

        Debug.Log($"Objective triggered: {newObjectiveText}");

        // Play optional dialogue
        if (dialogueTrigger != null)
        {
            dialogueTrigger.TriggerNow();
        }

        // Disable this trigger if needed
        if (disableAfterTrigger)
        {
            gameObject.SetActive(false);
        }
    }

    private void ShowPresetObjective()
    {
        switch (presetObjectiveNumber)
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
            case 4:
                hud.ShowObjective4();
                break;
            case 5:
                hud.ShowObjective5();
                break;
            case 6:
                hud.ShowObjective6();
                break;
            case 7:
                hud.ShowObjective7();
                break;
            case 8:
                hud.ShowObjective8();
                break;
            default:
                Debug.LogWarning($"Invalid preset objective number: {presetObjectiveNumber}");
                hud.ShowObjective(newObjectiveText);
                break;
        }
    }

    // Public method to manually trigger (can be called from other scripts)
    public void ManualTrigger()
    {
        TriggerObjective();
    }

    // Public method to reset the trigger
    public void ResetTrigger()
    {
        hasTriggered = false;
        gameObject.SetActive(true);
    }
}
