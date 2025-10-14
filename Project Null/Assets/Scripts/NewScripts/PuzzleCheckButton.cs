using System.Collections;
using UnityEngine;

public class PuzzleCheckButton : MonoBehaviour
{
    [Header("Settings")]
    public float interactRange = 2f;        // Distance player must be to interact
    public KeyCode interactKey = KeyCode.E;
    public float delayBeforeCheck = 3f;     // Seconds to wait before checking
    public Transform player;                // Assign player transform in inspector

    [Header("References")]
    public PuzzleFeedbackLight feedbackLight; // Assign the light script in Inspector
    public AudioSource buttonAudioSource;     // AudioSource component to play sound
    public AudioClip buttonClickSound;        // Sound clip for button press

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

        // Auto-find light if not manually assigned
        if (feedbackLight == null)
        {
            feedbackLight = GetComponentInChildren<PuzzleFeedbackLight>();
            if (feedbackLight == null)
                Debug.LogWarning("PuzzleCheckButton: No PuzzleFeedbackLight found in children!");
        }

        // Auto-assign AudioSource if missing
        if (buttonAudioSource == null)
        {
            buttonAudioSource = GetComponent<AudioSource>();
            if (buttonAudioSource == null)
            {
                buttonAudioSource = gameObject.AddComponent<AudioSource>();
                buttonAudioSource.playOnAwake = false;
            }
        }
    }

    void Update()
    {
        if (player == null || isChecking) return;

        float distance = Vector3.Distance(player.position, transform.position);
        if (distance <= interactRange && Input.GetKeyDown(interactKey))
        {
            // ✅ Play sound every time button is clicked
            if (buttonClickSound != null && buttonAudioSource != null)
                buttonAudioSource.PlayOneShot(buttonClickSound);

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

        yield return new WaitForSeconds(delayBeforeCheck);

        // Trigger the puzzle check
        if (DollPuzzleManager.Instance != null)
        {
            DollPuzzleManager.Instance.CheckPuzzle();
            bool isCorrect = DollPuzzleManager.Instance.lastPuzzleResult; // ✅ read result

            Debug.Log("Puzzle check complete! Result: " + (isCorrect ? "CORRECT" : "WRONG"));

            if (feedbackLight != null)
                feedbackLight.Flash(isCorrect);
        }
        else
        {
            Debug.LogError("PuzzleCheckButton: DollPuzzleManager.Instance is null!");
        }

        isChecking = false;
    }
}
