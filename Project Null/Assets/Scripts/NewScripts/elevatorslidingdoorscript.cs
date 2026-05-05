using System.Collections;
using UnityEngine;

public class SlidingDoorController : MonoBehaviour
{
    [Header("Sliding Door Settings")]
    [Tooltip("How far the door should move when opening (horizontal distance)")]
    public float slideDistance = 1.2f;

    [Tooltip("How fast the door slides open")]
    public float slideSpeed = 2f;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isAnimating = false;
    private bool hasOpened = false;

    void Start()
    {
        // Save initial closed position
        closedPosition = transform.position;

        //Automatically pick a slide direction along WORLD X axis (horizontal)
        // If the door is on the left (negative X), slide left
        // If the door is on the right (positive X), slide right
        float direction = (transform.position.x >= 0f) ? 1f : -1f;

        // Move along WORLD X only (horizontal)
        openPosition = closedPosition + new Vector3(0f, 0f, slideDistance * direction);
    }

    public void OpenDoor()
    {
        if (isAnimating || hasOpened) return;
        StartCoroutine(SlideTo(openPosition));
        hasOpened = true;
    }

    private IEnumerator SlideTo(Vector3 targetPos)
    {
        isAnimating = true;
        Vector3 startPos = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * slideSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        isAnimating = false;
    }
}
