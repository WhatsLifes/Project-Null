using System.Collections;
using UnityEngine;

public class SlidingDoorController : MonoBehaviour
{
    [Header("Sliding Door Settings")]
    public Vector3 closedPosition;
    public Vector3 openOffset;      // local movement offset when opening
    public float slideSpeed = 2f;
    public bool startsOpen = false;

    private Vector3 openPosition;
    private bool isOpen = false;
    private bool isAnimating = false;

    void Start()
    {
        if (startsOpen)
        {
            openPosition = transform.localPosition;
            closedPosition = openPosition - openOffset;
            transform.localPosition = openPosition;
            isOpen = true;
        }
        else
        {
            closedPosition = transform.localPosition;
            openPosition = closedPosition + openOffset;
            transform.localPosition = closedPosition;
            isOpen = false;
        }
    }

    public void OpenDoor()
    {
        if (isOpen || isAnimating) return;
        isOpen = true;
        StartCoroutine(SlideTo(openPosition));
    }

    public void CloseDoor()
    {
        if (!isOpen || isAnimating) return;
        isOpen = false;
        StartCoroutine(SlideTo(closedPosition));
    }

    private IEnumerator SlideTo(Vector3 targetPos)
    {
        isAnimating = true;
        Vector3 startPos = transform.localPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * slideSpeed;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.localPosition = targetPos;
        isAnimating = false;
    }
}
