using UnityEngine;
using UnityEngine.AI;

public class UniversalPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRange = 3f;
    public Transform holdPosition;
    public LayerMask pickableLayer;
    public Camera cam;
    private GameObject heldObject;
    private Rigidbody heldRb;
    private Collider heldCollider;

    void Awake()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
                Debug.LogError("UniversalPickup: No camera assigned or MainCamera found!");
        }

        // DEBUG: Check if holdPosition is assigned
        if (holdPosition == null)
        {
            Debug.LogError("UniversalPickup: holdPosition is NOT assigned in inspector!");
        }
        else
        {
            Debug.Log($"UniversalPickup: holdPosition is assigned to {holdPosition.name}");
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

    void LateUpdate()
    {
        if (heldObject != null && holdPosition != null)
        {
            // DEBUG: Log positions
            Debug.Log($"LateUpdate - Object: {heldObject.transform.position}, HoldPos: {holdPosition.position}");

            heldObject.transform.position = holdPosition.position;
            heldObject.transform.rotation = holdPosition.rotation;
        }
    }

    void TryPickup()
    {
        if (cam == null) return;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.red, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickableLayer))
        {
            Debug.Log($"Hit: {hit.collider.name}, Tag: {hit.collider.tag}");

            if (hit.collider.CompareTag("Pickable"))
            {
                heldObject = hit.collider.gameObject;
                heldRb = heldObject.GetComponent<Rigidbody>();
                heldCollider = heldObject.GetComponent<Collider>();

                Debug.Log($"=== PICKING UP {heldObject.name} ===");
                Debug.Log($"Has Rigidbody: {heldRb != null}");
                Debug.Log($"Has Collider: {heldCollider != null}");
                Debug.Log($"Initial Position: {heldObject.transform.position}");

                // If the doll is on a chair, remove it from the chair
                ChairSlot slot = heldObject.GetComponentInParent<ChairSlot>();
                if (slot != null)
                {
                    Debug.Log("Removing from chair");
                    slot.RemoveDollReference();
                }

                // Disable NavMeshAgent if it exists
                NavMeshAgent agent = heldObject.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    Debug.Log($"NavMeshAgent found - Enabled: {agent.enabled}");
                    if (agent.enabled && agent.isOnNavMesh)
                    {
                        agent.ResetPath();
                        agent.isStopped = true;
                    }
                    agent.enabled = false;
                    Debug.Log("NavMeshAgent disabled");
                }

                // Disable Enemy script if it exists
                Enemy enemy = heldObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Debug.Log("Enemy script found - deactivating");
                    enemy.Deactivate();
                }

                // Unparent first
                heldObject.transform.SetParent(null);
                Debug.Log("Unparented");

                if (heldRb != null)
                {
                    Debug.Log($"Rigidbody before: isKinematic={heldRb.isKinematic}, useGravity={heldRb.useGravity}");
                    heldRb.isKinematic = true;
                    heldRb.useGravity = false;
                    heldRb.constraints = RigidbodyConstraints.FreezeAll;
                    heldRb.linearVelocity = Vector3.zero;
                    heldRb.angularVelocity = Vector3.zero;
                    Debug.Log($"Rigidbody after: isKinematic={heldRb.isKinematic}, useGravity={heldRb.useGravity}");
                }

                if (heldCollider != null)
                {
                    Debug.Log("Disabling collider");
                    heldCollider.enabled = false;
                }

                // Set position immediately
                Debug.Log($"Setting position to holdPosition: {holdPosition.position}");
                heldObject.transform.position = holdPosition.position;
                heldObject.transform.rotation = holdPosition.rotation;
                Debug.Log($"Position after set: {heldObject.transform.position}");

                var doll = heldObject.GetComponent<DollBehavior>();
                if (doll != null)
                {
                    Debug.Log($"DollBehavior found - Type: {doll.type}");
                    doll.OnPickedUp();
                }

                Debug.Log($"=== PICKUP COMPLETE ===");

                // Optional: Hostile dolls immediately drop and chase
                if (doll != null && doll.type == DollBehavior.DollType.Hostile)
                {
                    Debug.Log("Hostile doll - dropping immediately");
                    Drop();
                }
            }
        }
        else
        {
            Debug.Log("Raycast hit nothing");
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

                if (heldCollider != null)
                    heldCollider.enabled = true;

                heldObject = null;
                heldRb = null;
                heldCollider = null;
                return;
            }
        }
        Drop();
    }

    void Drop()
    {
        if (heldObject == null) return;

        Debug.Log($"Dropping {heldObject.name}");

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
}