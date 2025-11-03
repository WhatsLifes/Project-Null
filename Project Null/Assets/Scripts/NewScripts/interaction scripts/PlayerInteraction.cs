using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private float generalDirectionAngle = 0.9f;

    [SerializeField] private LayerMask interactionLayer;

    [SerializeField] private List<InteractableObject> nearbyObjects = new List<InteractableObject>();
    private InteractableObject currentTarget;
    
    public static PlayerInteraction instance;

    public void Awake()
    {
        instance = this;
    }

    public void AddNearbyObject(InteractableObject obj)
    {
        if (!nearbyObjects.Contains(obj))
            nearbyObjects.Add(obj);
        
    }

    public void RemoveNearbyObject(InteractableObject obj)
    {
        if (nearbyObjects.Contains(obj))
            nearbyObjects.Remove(obj);
        
        if (currentTarget == obj)
            ClearCurrentTarget();
    }
    
    private void Update()
    {
        UpdateInteractions();

        if (Input.GetKeyDown(KeyCode.E) && currentTarget != null)
        {
            currentTarget.InteractItem();
            currentTarget = null;
        }
    }

    private void UpdateInteractions() // UpdateWhiteDots() in the tutorial
    {
        bool foundTarget = false;
        foreach (var obj in nearbyObjects)
        {
            Vector3 directionToObj = obj.transform.position - cameraTransform.position;
            float angleCheck = Vector3.Dot(cameraTransform.forward, directionToObj.normalized);

            if (angleCheck > generalDirectionAngle)
            {
                bool isLookingDirectly = isLookingDirectlyAt(obj);

                if (isLookingDirectly)
                {
                    if (currentTarget != obj)
                    {
                        currentTarget?.HidePrompt();
                        currentTarget = obj;
                        currentTarget.ShowPrompt();
                    }
                    
                    obj.HideWhiteDot();
                    foundTarget = true;
                }
                else
                {
                    obj.ShowWhiteDot();
                }
            }
            else
            {
                obj.HideWhiteDot();
            }
        }

        if (!foundTarget)
        {
            ClearCurrentTarget();
        }
    }

    private bool isLookingDirectlyAt(InteractableObject obj)
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red, 2f);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayer))
        {
            //Debug.Log(hit.collider.gameObject);
            return hit.collider.gameObject == obj.gameObject;
        }

        return false;
    }


    public void ClearCurrentTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.HidePrompt();
            currentTarget = null;
        }
    }
}
