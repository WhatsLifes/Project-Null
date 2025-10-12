using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamLookAt : MonoBehaviour
{
    public Transform cam, player;

    void Update()
    {
        cam.LookAt(player);
        cam.Rotate(-90, 90, 0);
    }
}
