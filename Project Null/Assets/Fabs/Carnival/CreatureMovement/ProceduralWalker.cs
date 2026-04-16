using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Leg
{
    public Transform ikTarget;
    public Transform hint;

    [HideInInspector] public Vector3 homeLocalPos;
    [HideInInspector] public bool isMoving;
    [HideInInspector] public Vector3 plantedPos;
}

public class ProceduralWalker : MonoBehaviour
{
    public List<Leg> legs = new List<Leg>();

    [Header("Step Settings")]
    public float stepDistance = 1.5f;
    public float stepHeight = 0.5f;
    public float stepSpeed = 4f;
    public int maxSimultaneousSteps = 2;
    public float cycleSpeed = 0.3f;
    public float StepHeightDistance = 10f;

    [Header("Step Scheduling")]
    public float stepCooldown = 0.1f;
    private float lastStepTime;
    private int currentLegIndex;

    [Header("Step Prediction")]
    public float stepOvershoot = 0.3f;

    private Vector3 lastBodyPos;
    private Vector3 velocity;

    void Start()
    {
        foreach (var leg in legs)
        {
            leg.homeLocalPos = transform.InverseTransformPoint(leg.ikTarget.position);
            leg.plantedPos = leg.ikTarget.position;
        }
    }

    void Update()
    {
        int movingCount = 0;

        velocity = (transform.position - lastBodyPos) / Time.deltaTime;
        lastBodyPos = transform.position;
        foreach (var leg in legs)
        {
            //leg.ikTarget.position += Vector3.up * Mathf.Sin(Time.time) * 0.001f;
            leg.ikTarget.position = leg.plantedPos;
        }

        foreach (var leg in legs)
            if (leg.isMoving) movingCount++;


        // ❌ cooldown gate (controls rhythm)
        if (Time.time - lastStepTime < stepCooldown)
            return;

        // 👉 only ONE controlled pass per tick
        TryScheduleStep(movingCount);
    }

    void TryScheduleStep(int movingCount)
    {
        int checks = legs.Count;

        for (int i = 0; i < checks; i++)
        {
            int index = (currentLegIndex + i) % legs.Count;
            Leg leg = legs[index];

            if (leg.isMoving)
                continue;

            if (movingCount >= maxSimultaneousSteps)
                return;

            Vector3 baseHome = transform.TransformPoint(leg.homeLocalPos);

            // 🔥 predictive overshoot based on movement
            Vector3 homeWorld = baseHome + velocity * 0.2f;

            Vector3 a = leg.plantedPos;
            Vector3 b = homeWorld;

            a.y = 0f;
            b.y = 0f;

            float dist = (a - b).sqrMagnitude;

            if (dist > stepDistance * stepDistance)
            {
                Vector3 target = GetGroundPoint(homeWorld);
                StartCoroutine(MoveLeg(leg, target));

                currentLegIndex = index + 1;
                lastStepTime = Time.time;

                return; // 🔥 only ONE leg per schedule tick
            }
        }
    }

    Vector3 GetGroundPoint(Vector3 point)
    {
        // 🔥 use body movement direction to improve stability
        Vector3 moveDir = velocity;
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude < 0.0001f)
            moveDir = transform.forward;

        moveDir.Normalize();

        // 🔥 slight forward bias so feet don’t lag behind
        Vector3 biasedPoint = point + moveDir * stepOvershoot;

        // 🔥 raycast start above predicted location
        Vector3 rayOrigin = biasedPoint + Vector3.up * 5f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, StepHeightDistance))
        {
            return hit.point;
        }

        return biasedPoint;
    }

    IEnumerator MoveLeg(Leg leg, Vector3 target)
    {
        leg.isMoving = true;

        Vector3 start = leg.plantedPos;

        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / cycleSpeed;

            Vector3 pos = Vector3.Lerp(start, target, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * stepHeight;

            leg.ikTarget.position = pos;

            yield return null;
        }

        leg.plantedPos = target; 
        leg.isMoving = false;
    }


    void OnDrawGizmos()
    {
        if (legs == null) return;

        foreach (var leg in legs)
        {
            if (leg == null || leg.ikTarget == null) continue;

            Vector3 baseHome = transform.TransformPoint(leg.homeLocalPos);

            // 🔥 predictive overshoot based on movement
            Vector3 homeWorld = baseHome + velocity * 0.2f;

            // 🟡 HOME (body anchor)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(homeWorld, stepDistance);

            // 🔵 PLANTED FOOT (real state)
            Vector3 planted = Application.isPlaying
                ? leg.plantedPos
                : leg.ikTarget.position;

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(planted, 0.05f);

            // 🔴 ERROR (this is what actually triggers stepping)
            Gizmos.color = Color.red;
            Gizmos.DrawLine(planted, homeWorld);

            float errorDist = Vector3.Distance(planted, homeWorld);

            // 🟠 ERROR RADIUS VISUAL
            Gizmos.color = errorDist > stepDistance ? Color.red : Color.green;
            Gizmos.DrawWireSphere(homeWorld, stepDistance);

            // 🔵 CURRENT IK POSITION
            Gizmos.color = leg.isMoving ? Color.magenta : Color.blue;
            Gizmos.DrawSphere(leg.ikTarget.position, 0.06f);

            // ⚪ RAYCAST DEBUG
            Vector3 rayStart = homeWorld + Vector3.up * 2f;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(rayStart, rayStart + Vector3.down * StepHeightDistance);

            if (Application.isPlaying)
            {
                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 5f))
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(hit.point, 0.05f);

                    // predicted step target
                    Gizmos.DrawLine(planted, hit.point);
                }
            }

            // 🟢 STEP ARC PREVIEW
            Gizmos.color = Color.green;

            Vector3 start = planted;
            Vector3 end = homeWorld + transform.forward * stepDistance;

            int resolution = 20;
            Vector3 prev = start;

            for (int i = 1; i <= resolution; i++)
            {
                float t = i / (float)resolution;

                Vector3 pos = Vector3.Lerp(start, end, t);
                pos.y += Mathf.Sin(t * Mathf.PI) * stepHeight;

                Gizmos.DrawLine(prev, pos);
                prev = pos;
            }
        }
    }

}