using System;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    [Header("Rotation Offset")] 
    [SerializeField] private float offsetX = 0f;
    [SerializeField] private float offsetY = 0f;
    [SerializeField] private float offsetZ = 0f;

    private void Update()
    {
        transform.LookAt(cameraTransform);
        
        transform.Rotate(offsetX, offsetY, offsetZ);
    }
}

