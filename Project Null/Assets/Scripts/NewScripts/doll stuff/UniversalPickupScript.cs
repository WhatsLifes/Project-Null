using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.AI;

public class UniversalPickup : MonoBehaviour
{
    [Header("Pickup Settings")] 
    public float pickupRange = 3f;
    public Transform holdPosition;
    public LayerMask pickableLayer;
    public LayerMask ChairLayer;
    public Camera cam;
    public GameObject InteractUI_Doll;
    public GameObject InteractUI_Chair;
    private GameObject heldObject;
    private Rigidbody heldRb;
    private Collider heldCollider;
    private bool obj4Shown = false;

    [SerializeField] private HUD hud;

    void Awake()
    {
        InteractUI_Doll.SetActive(false);
        InteractUI_Chair.SetActive(false);
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
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        
        // If holding something
        if (heldObject != null)
        {
            // Make sure the doll pickup text doesnt show up
            InteractUI_Doll.SetActive(false);

            // Raycast for chair
            if (Physics.Raycast(ray, out RaycastHit hitInfo, pickupRange, ChairLayer))
            {
                // Show chair interaction text
                InteractUI_Chair.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    TryPlaceOrDrop(hitInfo.collider.GetComponentInParent<ChairSlot>());   // Call the place/drop script and pass the chair information
                }
            }
            // This and the setActive(false) below were needed to not keep chair text
            else
            {
                InteractUI_Chair.SetActive(false);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Drop();
                }
            }

        }
        else
        {
            InteractUI_Chair.SetActive(false);
            // Raycast out to see if we're looking at a doll
            if (Physics.Raycast(ray, out RaycastHit hitInfo, pickupRange, pickableLayer))
            {
                // Show the pick up text
                InteractUI_Doll.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    TryPickup(hitInfo);
                }
            }
            else
            {
                InteractUI_Doll.SetActive(false);
            }
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
    
    void TryPickup(RaycastHit hit)
    {
        if (cam == null) return;

        if (hit.collider.CompareTag("Pickable"))
        {
            GameObject targetObject = hit.collider.gameObject;

            // CHECK IF HOSTILE DOLL FIRST - before any pickup logic
            var doll = targetObject.GetComponent<DollBehavior>();
            if (doll != null && doll.type == DollBehavior.DollType.Hostile)
            {
                Debug.Log("Hostile doll detected - triggering activation without pickup");
                doll.OnPickedUp(); // This will handle everything via ForceDropAndActivate()
                return; // DON'T continue with pickup logic
            }

            // NORMAL PICKUP LOGIC (for non-hostile dolls)
            heldObject = targetObject;
            heldRb = heldObject.GetComponent<Rigidbody>();
            heldCollider = heldObject.GetComponent<Collider>();
            if (!obj4Shown)
            {
                obj4Shown = true;
                hud.ShowObjective4();
            }


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

            // Notify doll it was picked up (for Audio dolls)
            if (doll != null)
            {
                Debug.Log($"DollBehavior found - Type: {doll.type}");
                doll.OnPickedUp();
            }

            Debug.Log($"=== PICKUP COMPLETE ===");
        }
    }



    void TryPlaceOrDrop(ChairSlot chair)
    {
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
    
