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

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float volume = 1f;
    public float wrongLineCooldown = 3f; // how long before wrongLine can play again
    [Header("Success Audio Player")]
    public AudioClip successMusic;     // Drop your .mp3 or .wav file here
    public bool playFullTrack = false; // optional toggle if you want long audio


    private bool isChecking = false;
    private bool hasPlayedCorrectLine = false; // ensures correct line only once
    private bool elevatorOpened = false;       // stops interaction after success
    private bool canPlayWrongLine = true;      // prevents spamming wrong line
    public bool ElevatorOpened => elevatorOpened;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

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
        if (useManagerCondition)
        {
            resolvedCondition = PickupDaughterPicture.daughterPiecePickedUp;
            Debug.Log("[ElevatorButtonFixed] Checking daughter piece: " +
                      "daughterPiecePickedUp=" + PickupDaughterPicture.daughterPiecePickedUp);
        }

        // 🔊 Handle audio and logic
        if (!resolvedCondition)
        {
            if (wrongLine != null && canPlayWrongLine)
            {
                audioSource.PlayOneShot(wrongLine, volume);
                canPlayWrongLine = false;
                StartCoroutine(ResetWrongLineCooldown());
            }
            else
            {
                Debug.Log("[ElevatorButtonFixed] Wrong line cooling down or missing.");
            }
        }
        else
        {
            if (!hasPlayedCorrectLine)
            {
                if (correctLine != null)
                    audioSource.PlayOneShot(correctLine, volume);

                // 🎵 If you want to play a full MP3 track (ambient or victory music)
                if (successMusic != null)
                {
                    audioSource.clip = successMusic;
                    audioSource.volume = volume;
                    audioSource.loop = playFullTrack; // loop it if needed
                    audioSource.Play();
                }

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

                elevatorOpened = true;
            }
        }

        if (interactUI != null)
            interactUI.SetActive(false);

        isChecking = false;
    }

    private IEnumerator ResetWrongLineCooldown()
    {
        yield return new WaitForSeconds(wrongLineCooldown);
        canPlayWrongLine = true;
    }
}
