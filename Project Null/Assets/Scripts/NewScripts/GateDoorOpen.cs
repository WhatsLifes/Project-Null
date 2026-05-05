using System.Collections;
using UnityEngine;

public class GateDoorOpen : MonoBehaviour, stage2_InteractableScript
{
    public Transform player;
    public AudioSource audioSource;

    public AudioClip lockedSound;
    public AudioClip openSound;

    public float interactRange = 3f;
    public float volume = 1f;
    public float lockedCooldown = 2f;

    public float openAngle = 90f;
    public float openSpeed = 2f;

    private bool gateOpened = false;
    private bool canPlayLockedSound = true;
    private bool isChecking = false;

    private Quaternion targetRotation;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        targetRotation = Quaternion.Euler(
            transform.eulerAngles.x,
            transform.eulerAngles.y + openAngle,
            transform.eulerAngles.z
        );
    }

    public void InteractScript()
    {
        if (isChecking || gateOpened)
            return;

        if (player == null)
            return;

        if (Vector3.Distance(player.position, transform.position) > interactRange)
            return;

        StartCoroutine(CheckGate());
    }

    IEnumerator CheckGate()
    {
        isChecking = true;

        bool hasKey = false;

        if (Stage2ProgressManager.Instance != null)
            hasKey = Stage2ProgressManager.Instance.gateKeyPickedUp;

        if (!hasKey)
        {
            Debug.Log("Player needs the gate key.");

            if (lockedSound != null && canPlayLockedSound)
            {
                audioSource.PlayOneShot(lockedSound, volume);
                Debug.Log("Lock sound played.");
                canPlayLockedSound = false;
                StartCoroutine(ResetLockedCooldown());
            }

            isChecking = false;
            yield break;
        }

        if (openSound != null)
            audioSource.PlayOneShot(openSound, volume);
            Debug.Log("Gate opened.");

        StartCoroutine(OpenDoor());

        gateOpened = true;
        isChecking = false;
    }

    IEnumerator OpenDoor()
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * openSpeed
            );

            yield return null;
        }

        transform.rotation = targetRotation;
    }

    IEnumerator ResetLockedCooldown()
    {
        yield return new WaitForSeconds(lockedCooldown);
        canPlayLockedSound = true;
    }
}