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
    
    [SerializeField] private PlayerHold playerHold;
    [SerializeField] private HUD hud;
    private bool obj4Shown = false;
    
    // WAKE UP
    public void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (playerHold == null)
            playerHold = GetComponent<PlayerHold>();
        
        if (playerHold == null)
            Debug.LogError("PlayerInteraction could not find PlayerHold script!");
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

        // if we hit E...
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentTarget != null)
            {
                // Interact with the object we are looking at
                currentTarget.InteractItem();
            }
            // call drop if we are holding a doll
            else if (playerHold != null && playerHold.IsHoldingObject())
            {
                Debug.Log("Dropping item");
                playerHold.Drop();
            }
        }
    }

    private void UpdateInteractions()
    {
        bool foundTarget = false;
        
        if (playerHold == null) return; 

        InteractableObject directHitTarget = null;
        
        // find what we are directly looking at
        foreach (var obj in nearbyObjects)
        {
            if (isLookingDirectlyAt(obj))
            {
                directHitTarget = obj;
                break;
            }
        }
        
        // now find that object
        foreach (var obj in nearbyObjects)
        {
            if (obj == directHitTarget)
            {
                obj.HideWhiteDot();
                foundTarget = true; 
                
                // special stuff for dealing with the chair and dolls
                bool shouldShow = true;
                if (obj.TryGetComponent<InteractableDoll>(out _))
                {
                    if (playerHold.IsHoldingObject()) shouldShow = false;
                    if (!obj4Shown)
                    {
                        obj4Shown = true;
                        hud.ShowObjective4();
                    }
                }
                else if (obj.TryGetComponent<InteractableChair>(out var chair))
                {
                    if (!playerHold.IsHoldingObject() || chair.IsSlotFilled()) shouldShow = false;
                }

                if (shouldShow)
                {
                    if (currentTarget != obj)
                    {
                        currentTarget?.HidePrompt();
                        currentTarget = obj;
                    }
                    currentTarget.ShowPrompt();
                }
                else
                {
                    // show its interactable but not right now
                    obj.ShowWhiteDot(); 
                    if (currentTarget == obj) 
                        ClearCurrentTarget();
                }
            }
            else
            {
                // not looking at
                obj.HidePrompt();
                
                Vector3 directionToObj = obj.transform.position - cameraTransform.position;
                float angleCheck = Vector3.Dot(cameraTransform.forward, directionToObj.normalized);
                if (angleCheck > generalDirectionAngle)
                {
                    obj.ShowWhiteDot();
                }
                else
                {
                    obj.HideWhiteDot();
                }
            }
        }

        if (!foundTarget)
            ClearCurrentTarget();
    }

    private bool isLookingDirectlyAt(InteractableObject obj)
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        //Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red, 2f);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayer, QueryTriggerInteraction.Collide))
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
