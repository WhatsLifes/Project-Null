using UnityEngine;

public class PlayerHold : MonoBehaviour
{
    public static PlayerHold Instance;

    [Header("Hold Position")]
    public Transform holdPosition; 
    
    public GameObject heldObject { get; private set; }
    private Rigidbody heldRb;
    private Collider heldCollider;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (holdPosition == null)
            Debug.LogError("PlayerHold: holdPosition is NOT assigned!");
    }

    void LateUpdate()
    {
        if (heldObject != null && holdPosition != null)
        {
            heldObject.transform.position = holdPosition.position;
            heldObject.transform.rotation = holdPosition.rotation;
        }
    }

    public bool IsHoldingObject()
    {
        return heldObject != null;
    }

    public void Pickup(GameObject objectToHold)
    {
        if (IsHoldingObject()) return; 

        heldObject = objectToHold;
        heldRb = heldObject.GetComponent<Rigidbody>();
        heldCollider = heldObject.GetComponent<Collider>();

        ChairSlot slot = heldObject.GetComponentInParent<ChairSlot>();
        if (slot != null)
        {
            slot.RemoveDollReference();
        }

        heldObject.transform.SetParent(null); // Unparent

        if (heldRb != null)
        {
            heldRb.isKinematic = true;
            heldRb.useGravity = false;
            heldRb.constraints = RigidbodyConstraints.FreezeAll;
        }

        if (heldCollider != null)
        {
            heldCollider.enabled = false;
        }

        if (heldObject.TryGetComponent<DollBehavior>(out var doll))
        {
            doll.OnPickedUp();
        }
    }

    public void Drop()
    {
        if (!IsHoldingObject()) return;

        if (heldRb != null)
        {
            heldRb.constraints = RigidbodyConstraints.None;
            heldRb.isKinematic = false;
            heldRb.useGravity = true;
        }

        if (heldCollider != null)
        {
            heldCollider.enabled = true;
        }

        heldObject = null;
        heldRb = null;
        heldCollider = null;
    }

    public GameObject Place()
    {
        if (!IsHoldingObject()) return null;

        GameObject objToPlace = heldObject;

        if (heldCollider != null)
        {
            heldCollider.enabled = true;
        }
        
        heldObject = null;
        heldRb = null;
        heldCollider = null;

        return objToPlace;
    }
}
