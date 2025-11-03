using System.Collections;
using UnityEngine;

public class ElevatorButtonFixed : MonoBehaviour
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
    public GameObject interactUI; // optional prompt

    [Header("Condition")]
    public bool useManagerCondition = false;
    public bool conditionMet = false;

    [Range(0f, 1f)] public float volume = 1f;

    private bool isChecking = false;
    private bool hasBeenUsed = false; // 🔒 Prevent spam

    void Start()
    {
        // Find player automatically if not assigned
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Ensure we have an AudioSource
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

    void Update()
    {
        if (player == null || isChecking || hasBeenUsed) return; // 🧱 ignore if already used

        float distance = Vector3.Distance(player.position, transform.position);

        // toggle interaction UI
        if (interactUI != null)
            interactUI.SetActive(distance <= interactRange);

        if (distance <= interactRange && Input.GetKeyDown(interactKey))
            StartCoroutine(HandleButtonPress());
    }

    private IEnumerator HandleButtonPress()
    {
        isChecking = true;
        hasBeenUsed = true; // ✅ lock interaction immediately

        bool resolvedCondition = conditionMet;
        if (useManagerCondition && DollPuzzleManager.Instance != null)
            resolvedCondition = DollPuzzleManager.Instance.lastPuzzleResult;

        Debug.Log("[ElevatorButtonFixed] Player pressed button. Condition = " + resolvedCondition);

        if (!resolvedCondition)
        {
            if (wrongLine != null)
                audioSource.PlayOneShot(wrongLine, volume);
            else
                Debug.LogWarning("No wrongLine assigned!");
        }
        else
        {
            if (correctLine != null)
                audioSource.PlayOneShot(correctLine, volume);

            Debug.Log("[ElevatorButtonFixed] Waiting " + waitBeforeOpen + " seconds before opening doors...");
            yield return new WaitForSeconds(waitBeforeOpen);

            // Open both doors
            if (leftDoor != null)
                leftDoor.SendMessage("OpenDoor", SendMessageOptions.DontRequireReceiver);
            if (rightDoor != null)
                rightDoor.SendMessage("OpenDoor", SendMessageOptions.DontRequireReceiver);
        }

        // Hide interact prompt permanently after use
        if (interactUI != null)
            interactUI.SetActive(false);

        isChecking = false;
    }
}
