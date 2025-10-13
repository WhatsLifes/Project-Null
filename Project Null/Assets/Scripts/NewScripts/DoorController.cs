using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform doorObject;              // The actual moving part of the door
    public float openHeight = 3f;             // How far up the door moves when opening
    public float openSpeed = 2f;              // How fast it moves
    public bool isOpen = false;

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool isAnimating = false;

    void Start()
    {
        if (doorObject == null)
            doorObject = transform;

        closedPos = doorObject.localPosition;
        openPos = closedPos + Vector3.up * openHeight;
    }

    public void OpenDoor()
    {
        if (isOpen || isAnimating) return;
        isOpen = true;
        StartCoroutine(OpenAnimation());
    }

    private IEnumerator OpenAnimation()
    {
        isAnimating = true;
        float t = 0f;
        Vector3 startPos = doorObject.localPosition;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            doorObject.localPosition = Vector3.Lerp(startPos, openPos, t);
            yield return null;
        }

        doorObject.localPosition = openPos;
        isAnimating = false;
        Debug.Log("Door opened upward!");
    }
}
