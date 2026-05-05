using UnityEngine;

public class AlignTerrain : MonoBehaviour
{ 
    RaycastHit hit;
    Vector3 theRay;

    public LayerMask terrainMask;

    void FixedUpdate()
    {
        Align();
    }
  
    private void Align()
    {
        theRay = -transform.up;

        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y, transform.position.z),
                theRay, out hit, 30, terrainMask))
        {

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.parent.rotation;

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime / 0.15f);
        }
    }
}