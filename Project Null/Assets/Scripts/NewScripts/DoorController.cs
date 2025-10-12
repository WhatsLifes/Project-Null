using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform doorObject;        // The actual moving part of the door
    public Vector3 openRotation = new Vector3(0, 90, 0); // The rotation when open
    public float openSpeed = 2f;
    public bool isOpen = false;

    private Quaternion closedRot;
    private Quaternion openRot;
    private bool isAnimating = false;

    void Start()
    {
        if (doorObject == null)
            doorObject = transform;

        closedRot = doorObject.localRotation;
        openRot = Quaternion.Euler(openRotation);
    }

    public void OpenDoor()
    {
        if (isOpen || isAnimating) return;
        isOpen = true;
        StartCoroutine(OpenAnimation());
    }

    private System.Collections.IEnumerator OpenAnimation()
    {
        isAnimating = true;
        float t = 0f;
        Quaternion startRot = doorObject.localRotation;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            doorObject.localRotation = Quaternion.Slerp(startRot, openRot, t);
            yield return null;
        }

        doorObject.localRotation = openRot;
        isAnimating = false;
        Debug.Log("Door opened!");
    }
}
