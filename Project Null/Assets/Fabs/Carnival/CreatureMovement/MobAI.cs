using UnityEngine;

public class MobAI : MonoBehaviour
{
    public Transform target;

    [Header("Chase Settings")]
    public float chaseSpeed = 5f;
    public float chaseDuration = 3f;

    [Header("Roam Settings")]
    public float roamSpeed = 2f;
    public float roamDuration = 4f;
    public float roamRadius = 5f;

    [Header("Turning")]
    public float turnSpeed = 5f;

    [Header("Height Offset")]
    public float heightOffset = 2f;

    [Header("Debug")]
    [SerializeField] private string currentStateDebug;

    private enum State { Chase, Roam }
    private State currentState;

    private float stateTimer;

    private Vector3 roamPoint;
    private Vector3 currentDirection;

    void Start()
    {
        currentDirection = transform.forward;
        SwitchState(State.Chase);
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;

        Vector3 desiredDir = Vector3.zero;

        if (currentState == State.Chase)
        {
            if (target)
            {
                Vector3 targetPos = target.position + Vector3.up * heightOffset;
                desiredDir = (targetPos - transform.position).normalized;
            }

            if (stateTimer <= 0f)
                SwitchState(State.Roam);
        }
        else
        {
            Vector3 roamPos = roamPoint;
            Vector3 toRoam = roamPos - transform.position;

            if (toRoam.magnitude < 0.5f)
                roamPoint = GetRandomRoamPoint();

            desiredDir = toRoam.normalized;

            if (stateTimer <= 0f)
                SwitchState(State.Chase);
        }

        // Smooth steering
        if (desiredDir != Vector3.zero)
        {
            currentDirection = Vector3.Lerp(
                currentDirection,
                desiredDir,
                turnSpeed * Time.deltaTime
            ).normalized;
        }

        // Move
        float speed = (currentState == State.Chase) ? chaseSpeed : roamSpeed;
        transform.position += currentDirection * speed * Time.deltaTime;

        // Face movement direction
        if (currentDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(currentDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }
    }

    void SwitchState(State newState)
    {
        currentState = newState;
        stateTimer = (newState == State.Chase) ? chaseDuration : roamDuration;

        currentStateDebug = currentState.ToString(); // 👈 shows in inspector

        if (newState == State.Roam)
            roamPoint = GetRandomRoamPoint();
    }

    Vector3 GetRandomRoamPoint()
    {
        Vector2 random = Random.insideUnitCircle * roamRadius;

        return new Vector3(
            transform.position.x + random.x,
            heightOffset,
            transform.position.z + random.y
        );
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, roamRadius);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(roamPoint, 0.2f);
            Gizmos.DrawLine(transform.position, roamPoint);

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, currentDirection * 2f);
        }

        Gizmos.color = Color.magenta;
        Vector3 heightPoint = transform.position + Vector3.up * heightOffset;
        Gizmos.DrawWireSphere(heightPoint, 0.2f);

        if (target)
        {
            Gizmos.color = Color.red;
            Vector3 targetPos = target.position + Vector3.up * heightOffset;
            Gizmos.DrawLine(transform.position, targetPos);
            Gizmos.DrawSphere(targetPos, 0.2f);
        }
    }
}