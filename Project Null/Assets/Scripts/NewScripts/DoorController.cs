using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float closedAngle = 0f;
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool startsOpen = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;

    private bool isAnimating = false;
    private Quaternion baseRotation;
    private Quaternion closedRot;
    private Quaternion openRot;

    void Start()
    {
        baseRotation = transform.localRotation;

        closedRot = baseRotation * Quaternion.Euler(0f, closedAngle, 0f);
        openRot   = baseRotation * Quaternion.Euler(0f, openAngle, 0f);

        transform.localRotation = startsOpen ? openRot : closedRot;
    }

    public void OpenDoor()
    {
        Debug.Log("OpenDoor called. isAnimating = " + isAnimating);

        if (!isAnimating)
        {
            StartCoroutine(RotateDoor(openRot));
        }
    }

    public void CloseDoor()
    {
        if (!isAnimating)
        {
            StartCoroutine(RotateDoor(closedRot));
        }
    }

    private IEnumerator RotateDoor(Quaternion targetRot)
    {
        isAnimating = true;

        Quaternion startRot = transform.localRotation;
        float t = 0f;

        // Play sound safely
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.localRotation = targetRot;
        isAnimating = false;
    }
}