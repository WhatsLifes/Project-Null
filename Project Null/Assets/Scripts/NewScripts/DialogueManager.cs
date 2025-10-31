using System.Collections;
using System.Collections.Generic; // <<-- needed for Queue<T>
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("References")]
    public AudioSource doctorAudioSource;
    public TMP_Text playerTextUI;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;
    public float textFadeDuration = 0.5f;

    private bool isPlaying = false;
    private Queue<DialogueSequence> queuedDialogues = new Queue<DialogueSequence>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool IsPlaying => isPlaying;
public void StartDialogue(DialogueSequence sequence)
{
    if (sequence == null) return;

    if (isPlaying)
    {
        queuedDialogues.Enqueue(sequence);
        return;
    }

    StartCoroutine(PlayDialogue(sequence));
}

    private IEnumerator PlayDialogue(DialogueSequence sequence)
    {
        isPlaying = true;

        foreach (DialogueLine line in sequence.lines)
        {
            // Fade out any existing text before new line
            yield return StartCoroutine(FadeOutText());

            if (line.isDoctor)
            {
                // Play the Doctor's audio line
                if (line.voiceClip != null)
                    doctorAudioSource.PlayOneShot(line.voiceClip);

                float waitTime = line.voiceClip != null ? line.voiceClip.length : 0f;
                yield return new WaitForSeconds(waitTime + line.delayBeforeNext);
            }
            else
            {
                // Type out the player's text
                yield return StartCoroutine(TypeText(line.playerText));
                yield return new WaitForSeconds(line.delayBeforeNext);
            }
        }

        // End of conversation — fade out text and mark not playing
        yield return StartCoroutine(FadeOutText());
        isPlaying = false;

        // If any queued dialogue exists, start it automatically
        if (queuedDialogues.Count > 0)
        {
            var next = queuedDialogues.Dequeue();
            StartCoroutine(PlayDialogue(next));
        }
    }
    private IEnumerator TypeText(string text)
{
    // Ensure text is visible and cleared first
    playerTextUI.text = "";
    Color col = playerTextUI.color;
    col.a = 1f;
    playerTextUI.color = col;

    foreach (char c in text)
    {
        playerTextUI.text += c;
        yield return new WaitForSeconds(typingSpeed);
    }
}

    private IEnumerator FadeOutText()
    {
        if (string.IsNullOrEmpty(playerTextUI.text))
            yield break;

        float elapsed = 0f;
        Color original = playerTextUI.color;

        while (elapsed < textFadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / textFadeDuration);
            playerTextUI.color = new Color(original.r, original.g, original.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Clear and restore alpha to full for the next line
        playerTextUI.text = "";
        playerTextUI.color = new Color(original.r, original.g, original.b, 1f);
    }
}