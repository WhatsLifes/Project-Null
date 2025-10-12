using UnityEngine;
using System.Collections.Generic;

public class DollPuzzleDebugger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Running Doll Puzzle Debugger...");

        //Use new Unity API (faster, no warnings)
        DollPuzzleManager manager = FindFirstObjectByType<DollPuzzleManager>();
        DollBehavior[] dolls = FindObjectsByType<DollBehavior>(FindObjectsSortMode.None);

        if (manager == null)
        {
            Debug.LogError("No DollPuzzleManager found in the scene!");
            return;
        }

        if (manager.chairs == null || manager.chairs.Count == 0)
        {
            Debug.LogError("No chairs registered in DollPuzzleManager!");
            return;
        }

        if (dolls.Length == 0)
        {
            Debug.LogWarning("No dolls found in scene!");
            return;
        }

        // Create easy-to-read lookup for chairs
        List<string> validPatterns = new List<string>();
        foreach (var chair in manager.chairs)
        {
            validPatterns.Add($"{chair.requiredLeftEye}-{chair.requiredRightEye}");
            Debug.Log($"Chair expects: Left='{chair.requiredLeftEye}', Right='{chair.requiredRightEye}'");
        }

        // Check each doll’s pattern
        foreach (var doll in dolls)
        {
            string dollPattern = $"{doll.leftEye}-{doll.rightEye}";
            bool matchesAny = validPatterns.Contains(dollPattern);

            if (string.IsNullOrEmpty(doll.leftEye) || string.IsNullOrEmpty(doll.rightEye))
            {
                Debug.LogWarning($"Doll '{doll.name}' has missing eye data! (Left='{doll.leftEye}', Right='{doll.rightEye}')");
            }
            else if (matchesAny)
            {
                Debug.Log($"Doll '{doll.name}' matches a valid chair pattern ({doll.leftEye}, {doll.rightEye})");
            }
            else
            {
                Debug.LogError($"Doll '{doll.name}' does NOT match any valid chair pattern! (Left='{doll.leftEye}', Right='{doll.rightEye}')");
            }
        }

        Debug.Log("Doll Puzzle Debug complete!");
    }
}
