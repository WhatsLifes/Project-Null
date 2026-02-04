using UnityEngine;

public class FootstepsScript : MonoBehaviour
{
    public GameObject footstepsObj;

    void Start()
    {
        footstepsObj.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKey("w"))
        {
            StartFootsteps();
        }

        if (Input.GetKeyDown("a"))
        {
            StartFootsteps();
        }

        if (Input.GetKeyDown("s"))
        {
            StartFootsteps();
        }

        if (Input.GetKeyDown("d"))
        {
            StartFootsteps();
        }

        if (Input.GetKeyUp("w"))
        {
            StopFootsteps();
        }

        if (Input.GetKeyUp("a"))
        {
            StopFootsteps();
        }

        if (Input.GetKeyUp("s"))
        {
            StopFootsteps();
        }

        if (Input.GetKeyUp("d"))
        {
            StopFootsteps();
        }
    }
    
    void StartFootsteps()
    {
        footstepsObj.SetActive(true);
    }

    void StopFootsteps()
    {
        footstepsObj.SetActive(false);
    }
}
