using UnityEngine;

interface IInteractable
{
    public void Interactt();
}
public class Interactor : MonoBehaviour
{
    public Transform target;
    public float interactionDistance = 3f;
    public LayerMask pickableLayer;
    void Update()
    {
        Ray ray = new Ray(target.position, target.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, pickableLayer))
        {
            
            if (Input.GetKeyDown(KeyCode.E))
            {

                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactObj))
                {
                    interactObj.Interactt();
                    
                }

            }
        }
    }
}
