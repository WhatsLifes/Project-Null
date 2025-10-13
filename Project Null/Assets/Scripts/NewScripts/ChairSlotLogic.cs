using UnityEngine;

public class ChairSlot : MonoBehaviour
{
    [HideInInspector] public GameObject currentDoll;
    private Transform placementPoint;

    void Awake()
    {
        // Find or create DollPlacementPoint
        placementPoint = transform.Find("DollPlacementPoint");
        if (placementPoint == null)
        {
            Debug.LogWarning($"ChairSlot on '{name}' could not find child 'DollPlacementPoint'. Creating one.");
            GameObject newPoint = new GameObject("DollPlacementPoint");
            newPoint.transform.SetParent(transform);
            newPoint.transform.localPosition = new Vector3(0, 0.5f, 0);
            newPoint.transform.localRotation = Quaternion.identity;
            placementPoint = newPoint.transform;
        }
    }

    public bool HasDoll() => currentDoll != null;

    public void PlaceDoll(GameObject doll)
    {
        if (doll == null) return;

        // Allow replacing doll only if it's the same doll
        if (currentDoll != null && currentDoll != doll)
        {
            Debug.Log($"{name} already has a doll.");
            return;
        }

        currentDoll = doll;

        //  STORE WORLD SCALE BEFORE PARENTING
        Vector3 worldScale = doll.transform.lossyScale;

        // Snap doll to placement point
        if (placementPoint != null)
        {
            doll.transform.SetParent(placementPoint, false);
            doll.transform.localPosition = Vector3.zero;
            doll.transform.localRotation = Quaternion.identity;

            //  RESTORE WORLD SCALE AS LOCAL SCALE
            // Calculate what local scale gives us the desired world scale
            if (placementPoint.lossyScale.x != 0 && placementPoint.lossyScale.y != 0 && placementPoint.lossyScale.z != 0)
            {
                doll.transform.localScale = new Vector3(
                    worldScale.x / placementPoint.lossyScale.x,
                    worldScale.y / placementPoint.lossyScale.y,
                    worldScale.z / placementPoint.lossyScale.z
                );
            }
        }

        // Lock physics while seated
        Rigidbody rb = doll.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Collider col = doll.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        // Notify doll
        DollBehavior behavior = doll.GetComponent<DollBehavior>();
        behavior?.OnPlaced();

        // Notify manager
        DollPuzzleManager.Instance?.OnDollPlaced(this);

        Debug.Log($"Placed doll '{doll.name}' on chair '{name}'. Scale preserved: {doll.transform.lossyScale}");
    }

    public void RemoveDollReference()
    {
        if (currentDoll == null) return;

        // Restore physics
        Rigidbody rb = currentDoll.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Collider col = currentDoll.GetComponent<Collider>();
        if (col != null) col.isTrigger = false;

        Debug.Log($"Removed doll '{currentDoll.name}' from chair '{name}'.");
        currentDoll = null;

        // Notify manager
        DollPuzzleManager.Instance?.OnDollRemoved(this);
    }
}
