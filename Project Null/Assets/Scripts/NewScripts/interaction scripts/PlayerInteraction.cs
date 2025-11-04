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
    
    // WAKE UP
    public void Awake()
    {
        instance = this;
    }

    // add nearby objects to the nearby object list
    public void AddNearbyObject(InteractableObject obj)
    {
        if (!nearbyObjects.Contains(obj))
            nearbyObjects.Add(obj);
    }

    // takes out objects from the nearby object list
    public void RemoveNearbyObject(InteractableObject obj)
    {
        if (nearbyObjects.Contains(obj))
            nearbyObjects.Remove(obj);
        
        // specifically takes a object out after interacting
        if (currentTarget == obj)
            ClearCurrentTarget();
    }
    
    private void Update()
    {
        UpdateInteractions();

        // if we hit E, run the interact script on the object we are looking at
        if (Input.GetKeyDown(KeyCode.E) && currentTarget != null)
        {
            currentTarget.InteractItem();
            currentTarget = null;
        }
    }

    private void UpdateInteractions()
    {
        
        bool foundTarget = false; // local variable for looking at objects
        
        // loops through all the objects in the nearby objects list
        foreach (var obj in nearbyObjects) 
        {
            // get the angle for looking at the object
            Vector3 directionToObj = obj.transform.position - cameraTransform.position;
            float angleCheck = Vector3.Dot(cameraTransform.forward, directionToObj.normalized);

            // see if we are looking at it
            if (angleCheck > generalDirectionAngle)
            {
                bool isLookingDirectly = isLookingDirectlyAt(obj);

                // if we are looking at it
                if (isLookingDirectly)
                {
                    if (currentTarget != obj)
                    {
                        currentTarget?.HidePrompt(); // null check
                        currentTarget = obj; // set the object as our target
                        currentTarget.ShowPrompt(); // show the correct prompt for that object
                    }
                    
                    // hide dot and sets found true
                    obj.HideWhiteDot();
                    foundTarget = true;
                }
                // if we arent looking directly, just how dot
                else
                    obj.ShowWhiteDot();
                
            }
            // hide dot if we arent vaugly looking there
            else
                obj.HideWhiteDot();
        }
        if (!foundTarget)
            ClearCurrentTarget();
    }

    private bool isLookingDirectlyAt(InteractableObject obj)
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        //Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red, 2f);
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
