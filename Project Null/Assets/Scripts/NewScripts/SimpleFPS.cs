using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPS : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;                 
    public float mouseSensitivity = 2f;      
    public Transform Camera;                 // Player camera (child)

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;          
    public float standHeight = 2f;           
    public float crouchSpeed = 2.5f;         
    public float crouchTransitionSpeed = 6f; 

    [Header("Attack Settings (TEST)")]
    public float attackRange = 3f;           
    public int damageAmount = 1;             
    public KeyCode attackKey = KeyCode.Mouse0;   
    public LayerMask dollLayer;              
    public bool showDebugRay = true;         

    private CharacterController controller;
    private float verticalLookRotation = 0f; 
    private bool isCrouching = false;        

    [Header("Prevent passing through dolls (optional)")]
    public LayerMask repelLayer;              
    public float repelStrength = 0.05f;      

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- Movement ---
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        float currentSpeed = isCrouching ? crouchSpeed : speed;
        controller.SimpleMove(move * currentSpeed);

        // --- Mouse Look ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        Camera.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);

        // --- Crouch ---
        if (Input.GetKey(KeyCode.LeftControl))
            isCrouching = true;
        else if (isCrouching && CanStandUp())
            isCrouching = false;

        float targetHeight = isCrouching ? crouchHeight : standHeight;
        controller.height = Mathf.MoveTowards(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        Vector3 camPos = Camera.localPosition;
        camPos.y = controller.height / 2f;
        Camera.localPosition = Vector3.MoveTowards(Camera.localPosition, camPos, Time.deltaTime * crouchTransitionSpeed);

        // --- Attack (TEST: SphereCastAll + debug logs) ---
        HandleAttackTest();

        // --- Repel / prevent going through dolls ---
        PreventPassThroughDolls();
    }

    private bool CanStandUp()
    {
        Vector3 basePos = controller.transform.position;
        float crouchTop = basePos.y + crouchHeight;
        float standTop = basePos.y + standHeight;

        Vector3 start = new Vector3(basePos.x, crouchTop, basePos.z);
        Vector3 end = new Vector3(basePos.x, standTop, basePos.z);
        float radius = controller.radius * 0.9f;

        return !Physics.CheckCapsule(start, end, radius, LayerMask.GetMask("Default"));
    }

    // --- TEST ATTACK: SphereCastAll + logs ---
    private void HandleAttackTest()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 rayOrigin = Camera.position + Camera.forward * 0.1f; // small forward offset
            Ray ray = new Ray(rayOrigin, Camera.forward);

            // Increase radius for testing
            float sphereRadius = 0.7f;

            // Ignore layer mask first to see everything (use dollLayer if you want to restrict)
            RaycastHit[] hits = Physics.SphereCastAll(rayOrigin, sphereRadius, Camera.forward, attackRange /*, dollLayer*/);

            bool hitSomething = false;
            Debug.Log($"[AttackTest] SphereCastAll hits: {hits.Length}");

            foreach (var hit in hits)
            {
                Debug.Log($"[AttackTest] Hit collider: {hit.collider.name} (distance {hit.distance})");

                // Try to find DollBehavior on the hit object or its parents
                DollBehavior doll = hit.collider.GetComponentInParent<DollBehavior>();
                if (doll != null)
                {
                    Debug.Log($"[AttackTest] Found DollBehavior on {doll.name} — calling TakeDamage()");
                    doll.TakeDamage(damageAmount);
                    Debug.Log($"Hit {doll.name} for {damageAmount} damage!");
                    hitSomething = true;
                    break; // stop after first doll hit
                }
                else
                {
                    Debug.Log($"[AttackTest] No DollBehavior on {hit.collider.name}");
                }
            }

            if (!hitSomething)
                Debug.Log("[AttackTest] No doll hit.");

            if (showDebugRay)
                Debug.DrawRay(rayOrigin, Camera.forward * attackRange, Color.red, 1f);
        }
    }

    // --- PREVENT PLAYER PASSING THROUGH (capsule-based) ---
    private void PreventPassThroughDolls()
    {
        if (repelLayer == 0) return; // if not set, skip

        Vector3 bottom = transform.position;
        Vector3 top = bottom + Vector3.up * controller.height;

        Collider[] hits = Physics.OverlapCapsule(bottom, top, controller.radius, repelLayer);
        foreach (var col in hits)
        {
            // skip if the overlap is trivial
            Vector3 closest = col.ClosestPoint(transform.position);
            Vector3 pushDir = (transform.position - closest);
            if (pushDir.sqrMagnitude < 0.0001f) continue;

            pushDir = pushDir.normalized;
            controller.Move(pushDir * repelStrength);
        }
    }

    // --- Crosshair ---
    private void OnGUI()
    {
        float size = 6f;
        float x = (Screen.width - size) / 2;
        float y = (Screen.height - size) / 2;
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(x, y, size, size), Texture2D.whiteTexture);
    }
}
