using System.Collections;
using UnityEngine;

public class PuzzleCheckButton : MonoBehaviour
{
    [Header("Settings")]
    public float interactRange = 2f;        // Distance player must be to interact
    public KeyCode interactKey = KeyCode.E;
    public float delayBeforeCheck = 3f;     // Seconds to wait before checking
    public Transform player;                // Assign player transform in inspector

    private bool isChecking = false;

    void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else Debug.LogError("PuzzleCheckButton: Player not assigned and not found in scene!");
        }
    }

    void Update()
    {
        if (player == null || isChecking) return;

        float distance = Vector3.Distance(player.position, transform.position);
        if (distance <= interactRange && Input.GetKeyDown(interactKey))
        {
            // Ensure all chairs are filled before allowing check
            if (DollPuzzleManager.Instance != null && DollPuzzleManager.Instance.AreAllChairsFilled())
            {
                StartCoroutine(DoPuzzleCheck());
            }
            else
            {
                Debug.Log("Cannot check puzzle yet: all chairs must have dolls.");
            }
        }
    }

    IEnumerator DoPuzzleCheck()
    {
        isChecking = true;
        Debug.Log("Button pressed! Waiting " + delayBeforeCheck + " seconds before checking...");

        // Optional: play button press animation or sound here
        yield return new WaitForSeconds(delayBeforeCheck);

        // Trigger the puzzle check
        if (DollPuzzleManager.Instance != null)
        {
            DollPuzzleManager.Instance.CheckPuzzle();
            Debug.Log("Puzzle check complete!");
        }
        else
        {
            Debug.LogError("PuzzleCheckButton: DollPuzzleManager.Instance is null!");
        }

        isChecking = false;
    }
}
