using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueSequence dialogueToPlay;

    [Header("Trigger Options")]
    public bool playOnStart = false;      // play automatically when scene starts
    [Tooltip("Seconds to wait before auto-triggering when playOnStart is true")]
    public float startDelay = 6f;

    public bool playOnce = true;         // only allow triggering once
    public bool destroyAfterTrigger = false;

    [Header("Queueing & Priority")]
    public bool queueIfManagerBusy = true; // enqueue if DialogueManager is already playing

    private bool hasTriggered = false;
    private Coroutine startDelayCoroutine;

    void Start()
    {
        // Start with a coroutine to ensure the delay works properly
        if (playOnStart)
        {
            // If already started and want to delay, run coroutine
            startDelayCoroutine = StartCoroutine(DelayedAutoStart(startDelay));
        }
    }

    private IEnumerator DelayedAutoStart(float delay)
    {
        // Wait for delay (unscaled time)
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        TryTriggerDialogue();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger when Player enters (change tag if your player uses another)
        if (!other.CompareTag("Player")) return;

        TryTriggerDialogue();
    }
    /// Public method to attempt triggering the dialogue.
    /// Respects playOnce and queueIfManagerBusy.
    public void TryTriggerDialogue()
    {
        if (hasTriggered && playOnce) return;
        if (dialogueToPlay == null)
        {
            Debug.LogWarning($"DialogueTrigger on '{gameObject.name}' has no DialogueSequence assigned.");
            return;
        }
// If DialogueManager doesn't exist, attempt to start anyway (will throw null if missing)
        var dm = DialogueManager.Instance;
        if (dm == null)
        {
            Debug.LogError("DialogueManager.Instance is null. Make sure a DialogueManager exists in the scene.");
            return;
        }

        // If manager is busy:
        if (dm.IsPlaying)
        {
            if (queueIfManagerBusy)
            {
                dm.StartDialogue(dialogueToPlay); // our manager enqueues automatically if busy
                PostTriggerCleanup();
            }
            else
            {
                // Skip starting since manager is busy and we don't want to queue
                return;
            }
        }
        else
        {
            dm.StartDialogue(dialogueToPlay);
            PostTriggerCleanup();
        }
    }

    private void PostTriggerCleanup()
    {
        hasTriggered = true;
        // Stop the auto-start coroutine if it's still running
        if (startDelayCoroutine != null)
        {
            StopCoroutine(startDelayCoroutine);
            startDelayCoroutine = null;
        }

        if (destroyAfterTrigger)
            Destroy(gameObject);
    }

    // Optional helper so other scripts can call this trigger directly
    public void TriggerNow()
    {
        TryTriggerDialogue();
    }

    // If you want to reset the trigger for testing in-editor:
    #if UNITY_EDITOR
    [ContextMenu("Reset Trigger")]
    public void ResetTrigger()
    {
        hasTriggered = false;
        if (startDelayCoroutine != null)
        {
            StopCoroutine(startDelayCoroutine);
            startDelayCoroutine = null;
        }
    }
    #endif
}