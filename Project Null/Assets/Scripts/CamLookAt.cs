using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamLookAt : MonoBehaviour
{
    public Transform cam;
    public Transform player;

    void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                Debug.LogWarning("Player with tag 'Player' not found in scene!");
            }
        }
    }

    void Update()
    {
        if (cam != null && player != null)
        {
            cam.LookAt(player);
            cam.Rotate(-90, 90, 0);
        }
    }
}
