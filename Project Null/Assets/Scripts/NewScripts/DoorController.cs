using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public float closedAngle = 0f;  // relative rotation for closed state
    public float openAngle = 90f;   // relative rotation for open state
    public float openSpeed = 2f;
    public bool startsOpen = false;   // does door start open?

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;

    private Quaternion closedRot;
    private Quaternion openRot;
    private bool isOpen = false;
    private bool isAnimating = false;

    void Start()
    {
        // Record the current rotation as the "closed" rotation
        closedRot = transform.rotation * Quaternion.Euler(0f, closedAngle, 0f);
        openRot = transform.rotation * Quaternion.Euler(0f, openAngle, 0f);

        // Set starting rotation
        if (startsOpen)
        {
            transform.rotation = openRot;
            isOpen = true;
        }
        else
        {
            transform.rotation = closedRot;
            isOpen = false;
        }
    }

    public void OpenDoor()
    {
        if (isOpen || isAnimating) return;
        isOpen = true;
        StartCoroutine(RotateDoor(openRot));
    }

    public void CloseDoor()
    {
        if (!isOpen || isAnimating) return;
        isOpen = false;
        StartCoroutine(RotateDoor(closedRot));
    }

    private IEnumerator RotateDoor(Quaternion targetRot)
    {
        isAnimating = true;
        float t = 0f;
        Quaternion startRot = transform.rotation;

        audioSource.PlayOneShot(openSound);
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.rotation = targetRot;
        isAnimating = false;
        Debug.Log(isOpen ? "Door opened!" : "Door closed!");
    }
}
