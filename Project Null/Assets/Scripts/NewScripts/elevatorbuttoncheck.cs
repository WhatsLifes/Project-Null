using System.Collections;
using UnityEngine;

public class ElevatorButtonFixed : MonoBehaviour, InteractableScript
{
    [Header("Button Settings")]
    public float waitBeforeOpen = 15f;
    public KeyCode interactKey = KeyCode.E;

    [Header("References")]
    public Transform player;
    public MonoBehaviour leftDoor;
    public MonoBehaviour rightDoor;
    public AudioSource audioSource;
    public AudioClip correctLine;
    public AudioClip wrongLine;

    [Header("Interaction")]
    public float interactRange = 3f;
    public GameObject interactUI; // Optional interaction prompt

    [Header("Condition")]
    public bool useManagerCondition = false;
    public bool conditionMet = false; // Local fallback if manager not used

    [Range(0f, 1f)] public float volume = 1f;

    private bool isChecking = false;
    private bool hasPlayedWrongLine = false;  // ensures wrong line only once
    private bool hasPlayedCorrectLine = false; // ensures correct line only once
    private bool elevatorOpened = false;      // stops interaction after success

    void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Make sure we have an AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        if (interactUI != null)
            interactUI.SetActive(false);
    }

    public void InteractScript()
    {
        if (player == null || isChecking || elevatorOpened) return;
        
        StartCoroutine(HandleButtonPress());
    }

    private IEnumerator HandleButtonPress()
    {
        isChecking = true;

        bool resolvedCondition = conditionMet;

        // ✅ Use GameProgressManager instead of DollPuzzleManager
        if (useManagerCondition && GameProgressManager.Instance != null)
        {
            resolvedCondition = GameProgressManager.Instance.CanOpenFinalDoor();
            Debug.Log("[ElevatorButtonFixed] Checking GameProgressManager: " +
                      "buttonPressed=" + GameProgressManager.Instance.buttonPressed +
                      ", puzzleCompleted=" + GameProgressManager.Instance.puzzleCompleted +
                      " → CanOpenFinalDoor=" + resolvedCondition);
        }

        // 🔊 Handle audio and logic
        if (!resolvedCondition)
        {
            // Only play wrong line the first time it happens
            if (!hasPlayedWrongLine && wrongLine != null)
            {
                audioSource.PlayOneShot(wrongLine, volume);
                hasPlayedWrongLine = true;
            }
            else
            {
                Debug.Log("[ElevatorButtonFixed] Condition not met — no repeat audio.");
            }
        }
        else
        {
            // Only play correct line once and open doors
            if (!hasPlayedCorrectLine && correctLine != null)
            {
                audioSource.PlayOneShot(correctLine, volume);
                hasPlayedCorrectLine = true;
            }

            if (!elevatorOpened)
            {
                Debug.Log("[ElevatorButtonFixed] Condition met. Opening doors after " + waitBeforeOpen + " seconds...");
                yield return new WaitForSeconds(waitBeforeOpen);

                if (leftDoor != null)
                    leftDoor.SendMessage("OpenDoor", SendMessageOptions.DontRequireReceiver);
                if (rightDoor != null)
                    rightDoor.SendMessage("OpenDoor", SendMessageOptions.DontRequireReceiver);

                elevatorOpened = true; // ✅ locks out further presses after success
            }
        }

        if (interactUI != null)
            interactUI.SetActive(false);

        isChecking = false;
    }
}
