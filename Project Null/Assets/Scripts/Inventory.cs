using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private Sanity sanity;

    public KeyCode useKey = KeyCode.Q;

    [Header("Things in Inventory")]
    public static bool holdingSyringe; // persists across scenes
    public int pictureCount;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip injectSyringe;

    void Start()
    {
        // DO NOT reset holdingSyringe

        if (sanity == null)
        {
            sanity = FindObjectOfType<Sanity>();

            if (sanity == null)
            {
                Debug.LogError("Inventory: Sanity not found in scene");
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(useKey))
        {
            Debug.Log("Inventory: Q pressed");
            useSyringe();
        }
    }

    public void pickUpSyringe()
    {
        holdingSyringe = true;
    }

    public void pickUpPicture()
    {
        pictureCount++;
    }

    void useSyringe()
    {
        Debug.Log("Inventory: Attempting to use syringe");

        if (!holdingSyringe)
        {
            Debug.Log("Inventory: NOT HOLDING SYRINGE");
            return;
        }

        Debug.Log("Inventory: Using syringe");

        if (sanity == null)
        {
            Debug.LogError("Inventory: Sanity is NULL");
            return;
        }

        if (audioSource != null && injectSyringe != null)
        {
            audioSource.PlayOneShot(injectSyringe);
        }
        else
        {
            Debug.LogWarning("Inventory: AudioSource or clip missing");
        }

        sanity.RestoreSanity(100f);

        holdingSyringe = false;
    }
}