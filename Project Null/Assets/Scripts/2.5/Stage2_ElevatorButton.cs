using System.Collections;
using UnityEngine;

public class Stage2_5ElevatorButton : MonoBehaviour, InteractableScript
{
    public float waitBeforeOpen = 15f;

    public Transform player;
    public MonoBehaviour leftDoor;
    public MonoBehaviour rightDoor;
    public AudioSource audioSource;

    public AudioClip correctLine;
    public AudioClip wrongLine;

    public float interactRange = 3f;
    public GameObject interactUI;

    public float volume = 1f;
    public float wrongLineCooldown = 3f;

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
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        if (interactUI != null)
            interactUI.SetActive(false);
    }

    public void InteractScript()
    {
        if (isChecking || elevatorOpened)
            return;

        if (player == null || Vector3.Distance(player.position, transform.position) > interactRange)
        {
            Debug.Log("Player too far from the elevator button.");
            return;
        }

        StartCoroutine(HandleButtonPress());
    }

    private IEnumerator HandleButtonPress()
    {
        isChecking = true;

        bool conditionMet = Stage2_5ProgressManager.Instance.AllFlowersCollected();

        // ❌ Not ready
        if (!conditionMet)
        {
            Debug.Log("Both flowers not collected — cannot open elevator.");

            if (wrongLine != null && canPlayWrongLine)
            {
                audioSource.PlayOneShot(wrongLine, volume);
                canPlayWrongLine = false;
                StartCoroutine(ResetWrongLineCooldown());
            }

            isChecking = false;
            yield break;
        }

        // ✅ Ready
        Debug.Log("Both flowers collected — opening elevator!");

        if (!hasPlayedCorrectLine && correctLine != null)
            audioSource.PlayOneShot(correctLine, volume);

        if (successMusic != null)
        {
            audioSource.clip = successMusic;
            audioSource.loop = playFullTrack;
            audioSource.volume = volume;
            audioSource.Play();
        }

        hasPlayedCorrectLine = true;

        yield return new WaitForSeconds(waitBeforeOpen);

        if (leftDoor != null)
            leftDoor.SendMessage("OpenDoor", SendMessageOptions.DontRequireReceiver);

        if (rightDoor != null)
            rightDoor.SendMessage("OpenDoor", SendMessageOptions.DontRequireReceiver);

        elevatorOpened = true;

        if (interactUI != null)
            interactUI.SetActive(false);

        isChecking = false;
    }

    IEnumerator ResetWrongLineCooldown()
    {
        yield return new WaitForSeconds(wrongLineCooldown);
        canPlayWrongLine = true;
    }
}
