using System.Collections;
using UnityEngine;

public class Stage2ElevatorButton : MonoBehaviour, InteractableScript
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
    public GameObject interactUI;

    [Header("Condition")]
    public bool useManagerCondition = false;
    public bool conditionMet = false;

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float volume = 1f;
    public float wrongLineCooldown = 3f;

    [Header("Success Audio Player")]
    public AudioClip successMusic;
    public bool playFullTrack = false;

    private bool isChecking = false;
    private bool hasPlayedCorrectLine = false;
    private bool elevatorOpened = false;
    private bool canPlayWrongLine = true;

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

        if (useManagerCondition)
        {
            resolvedCondition =
                Stage2ProgressManager.Instance != null &&
                Stage2ProgressManager.Instance.sonPhotoPickedUp;

            Debug.Log("[Stage2ElevatorButton] SON PHOTO = " +
                      Stage2ProgressManager.Instance.sonPhotoPickedUp);
        }

        if (!resolvedCondition)
        {
            if (wrongLine != null && canPlayWrongLine)
            {
                audioSource.PlayOneShot(wrongLine, volume);
                canPlayWrongLine = false;
                StartCoroutine(ResetWrongLineCooldown());
            }
        }
        else
        {
            if (!hasPlayedCorrectLine)
            {
                if (correctLine != null)
                    audioSource.PlayOneShot(correctLine, volume);

                if (successMusic != null)
                {
                    audioSource.clip = successMusic;
                    audioSource.volume = volume;
                    audioSource.loop = playFullTrack;
                    audioSource.Play();
                }

                hasPlayedCorrectLine = true;
            }

            if (!elevatorOpened)
            {
                Debug.Log("[Stage2ElevatorButton] Opening doors in " + waitBeforeOpen + " seconds...");
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
