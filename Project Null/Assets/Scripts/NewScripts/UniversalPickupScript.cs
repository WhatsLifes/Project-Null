using UnityEngine;

public class UniversalPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRange = 3f;
    public Transform holdPosition;
    public LayerMask pickableLayer;
    public Camera cam;

    private GameObject heldObject;
    private Rigidbody heldRb;

    void Awake()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
                Debug.LogError("UniversalPickup: No camera assigned or MainCamera found!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
                TryPickup();
            else
                TryPlaceOrDrop();
        }
    }

    void TryPickup()
    {
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickableLayer))
        {
            if (hit.collider.CompareTag("Pickable"))
            {
                heldObject = hit.collider.gameObject;
                heldRb = heldObject.GetComponent<Rigidbody>();

                // If the doll is on a chair, remove it from the chair
                ChairSlot slot = heldObject.GetComponentInParent<ChairSlot>();
                if (slot != null)
                {
                    slot.RemoveDollReference();
                }

                if (heldRb != null)
                {
                    heldRb.useGravity = false;
                    heldRb.isKinematic = true;
                    heldRb.constraints = RigidbodyConstraints.None; // unlock any previous constraints
                }

                Collider col = heldObject.GetComponent<Collider>();
                if (col != null)
                    col.isTrigger = false;

                heldObject.transform.SetParent(holdPosition);
                heldObject.transform.localPosition = Vector3.zero;
                heldObject.transform.localRotation = Quaternion.identity;

                var doll = heldObject.GetComponent<DollBehavior>();
                if (doll != null)
                    doll.OnPickedUp();

                Debug.Log($"Picked up {heldObject.name}");

                // Optional: Hostile dolls immediately drop and chase
                if (doll != null && doll.type == DollBehavior.DollType.Hostile)
                {
                    Drop();
                }
            }
        }
    }

    void TryPlaceOrDrop()
    {
        if (cam == null || heldObject == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            ChairSlot chair = hit.collider.GetComponentInParent<ChairSlot>();
            if (chair != null && !chair.HasDoll())
            {
                chair.PlaceDoll(heldObject);
                heldObject = null;
                heldRb = null;
                return;
            }
        }

        Drop();
    }

    void Drop()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);

        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldRb.useGravity = true;
            heldRb.constraints = RigidbodyConstraints.None;
        }

        heldObject = null;
        heldRb = null;
    }
}
