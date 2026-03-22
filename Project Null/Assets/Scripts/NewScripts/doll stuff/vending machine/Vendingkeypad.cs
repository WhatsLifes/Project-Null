using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Audio;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Collections;

public class Vendingkeypad : MonoBehaviour, InteractableScript
{
    [Header("Ui Element")] 
    public GameObject KeypadCanvas;
    public TMP_Text inputField;
    
    [Header("Player")]
    [SerializeField] MonoBehaviour playerLookScript;
    [SerializeField] MonoBehaviour CamerabobScript;

    [Header("Audio Elements")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip successfulCode; // sounds for successful codes
    [SerializeField] private AudioClip buttonInteract; // sound for pressing buttons
    [SerializeField] private AudioClip unsuccessfulCode;
    [SerializeField] private AudioClip machineDoorOpenClose; // sound for vending machine door sliding open/close

    [Header("Item Spawned")] 
    [SerializeField] private bool Syringe_Spawned = false;
    [SerializeField] private bool Doll_XT = false;
    [SerializeField] private bool Doll_TS = false;
    [SerializeField] private bool Doll_OX = false;
    [SerializeField] private bool HostileDoll1 = false;
    [SerializeField] private bool HostileDoll2 = false;

    [Header("Objects")] [Tooltip("Things we want to script to spawn/use")]
    [SerializeField] private GameObject Syringe;
    [SerializeField] private GameObject Hostile_Doll1;
    [SerializeField] private GameObject Hostile_Doll2;
    [SerializeField] private GameObject DollEye_XT;
    [SerializeField] private GameObject DollEye_TS;
    [SerializeField] private GameObject DollEye_OX;
    
    [Header("Vending Port")]
    [SerializeField] private vendingport vendingport;
    
    // different conditions
    public static bool IsPaused = false;
    public static bool IsOpen = false;
    public static bool First_char = false;

    // check the amount of inputs
    private int buttonClicked = 0;
    
    // positions and rotations for teleporting things
    private Vector3 Doll_position = new Vector3(-17.77f, 1.043f, 7.507f);
    private Vector3 Doll_rotation = new Vector3(-0.751f, 179.852f, 1.891f);
    private Vector3 syringe_position = new Vector3(-17.969f, 0.325f, 7.32f);
    
    void Start()
    {
        // start with the keypad canvas off
        if (KeypadCanvas != null)
            KeypadCanvas.SetActive(false);

        Time.timeScale = 1f;
        IsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    
    // interact script
    // check if the receptacle is open
    public void InteractScript()
    {
        if(!IsOpen)
        {
            Pause();
        }
    }

    // timer to make sure we can only interact when receptacle is closed
    IEnumerator timer(float duration)
    {
        yield return new WaitForSeconds(duration);
        audioSource.PlayOneShot(machineDoorOpenClose);
        IsOpen = false;
    }
    
    // pause everything
    // like headbobbing
    public void Pause()
    {
        if (KeypadCanvas != null)
            KeypadCanvas.SetActive(true);

        Time.timeScale = 0f;
        IsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerLookScript != null)
            playerLookScript.enabled = false;
        if (CamerabobScript != null)
            CamerabobScript.enabled = false;
    }

    // unpause everything
    public void Resume()
    {
        if (KeypadCanvas != null)
            KeypadCanvas.SetActive(false);

        Time.timeScale = 1f;
        IsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerLookScript != null)
            playerLookScript.enabled = true;
        if (CamerabobScript != null)
            CamerabobScript.enabled = true;
    }
    
    // check the keycode inputed
    public void CheckCode(string code)
    {
        // only if the keycode is 1 char and 2 digits
        if(code.Length == 3)
        {
            switch (code)
            {
                // codes for dolls
                case "A12":
                    if (!Doll_XT)
                    {
                        DollEye_XT.transform.localPosition = Doll_position;
                        DollEye_XT.transform.localRotation = Quaternion.Euler(Doll_rotation);
                        Doll_XT = true;
                    }
                    break;
                case "B74":
                    if (!Doll_TS)
                    {
                        DollEye_OX.transform.localPosition = Doll_position;
                        DollEye_OX.transform.localRotation = Quaternion.Euler(Doll_rotation);
                        Doll_OX = true;
                    }
                    break;
                case "B49":
                    if (!Doll_OX)
                    {
                        DollEye_TS.transform.localPosition = Doll_position;
                        DollEye_TS.transform.localRotation = Quaternion.Euler(Doll_rotation);
                        Doll_TS = true;
                    }
                    break;
                // randomly choose something or do nothing
                default:
                    int random = Random.Range(0, 4);
                    Debug.Log("the random number is: " + random);
                    // teleports hostile doll 1
                    if (random == 1 && !HostileDoll1)
                    {
                        Hostile_Doll1.transform.localPosition = Doll_position;
                        Hostile_Doll1.transform.localRotation = Quaternion.Euler(Doll_rotation);
                        HostileDoll1 = true;
                    }
                    // teleports syringe
                    else if (random == 2 && !Syringe_Spawned)
                    {
                        Syringe.transform.localPosition = syringe_position;
                        Syringe_Spawned = true;
                    }
                    // teleports hostile doll 2
                    else if (random == 3 && !HostileDoll2)
                    {
                        Hostile_Doll2.transform.localPosition = Doll_position;
                        Hostile_Doll2.transform.localRotation = Quaternion.Euler(Doll_rotation);
                        HostileDoll2 = true;
                    }
                    // do nothing lol
                    break;
            }
            // clear the keypad 
            inputField.text = "";
            // open the receptacle
            vendingport.Open();
            // play the sound
            audioSource.PlayOneShot(successfulCode);
            audioSource.PlayOneShot(machineDoorOpenClose);
            IsOpen = true;
            Resume();
            // start timer to close the receptacle
            StartCoroutine(timer(5f));
        }
        // code too short
        else
        {
            audioSource.PlayOneShot(unsuccessfulCode);
        }
    }
    
    // keypad stuff
    public void ValueEntered(string value)
    {
        // play sound of pressing buttons
        audioSource.PlayOneShot(buttonInteract);
        switch (value)
        {
            // clearing inputs
            case "Clear":  
                inputField.text = "";
                buttonClicked = 0;
                break;
            // quit using keypad
            case "Quit":
                Resume();
                break;
            // check the code entered
            case "Enter":
                if (buttonClicked == 3)
                {
                    CheckCode(inputField.text);
                }
                buttonClicked = 0;
                break;
            // input whatever
            default:
                // only if theres less than 3 inputs
                if (buttonClicked < 3)
                {
                    // check if the first input is a letter
                    if (buttonClicked == 0 && char.IsLetter(value[0]))
                    {
                        buttonClicked++;
                        inputField.text += value;
                    }
                    // check if everything else is a number
                    else if (buttonClicked > 0 && char.IsDigit(value[0]))
                    {
                        buttonClicked++;
                        inputField.text += value;
                    }
                    // if dupe letter or no letter first
                    else
                    {
                        audioSource.PlayOneShot(unsuccessfulCode);
                    }
                }
                break;
        }
    }
    
}
