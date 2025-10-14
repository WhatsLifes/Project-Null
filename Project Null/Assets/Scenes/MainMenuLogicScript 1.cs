using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuLogicScript1 : MonoBehaviour

{
    public GameObject MainMenu;
    public GameObject SettingsMenu;

    private void Start()
    {
        if (MainMenu == null || SettingsMenu == null)
        {
            Debug.LogError("Main Menu or Settings Menu not assigned in Inspector!");
            return;
        }

        MainMenu.SetActive(true);
        SettingsMenu.SetActive(false);
    }

    public void StartButtom()
    {
        MainMenu.GetComponent<Canvas>().enabled = false;
        SceneManager.LoadScene("SampleScene");
    }

    public void SettingsButton()
    {
        MainMenu.GetComponent<Canvas>().enabled = false;
        SettingsMenu.GetComponent<Canvas>().enabled = true;
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void
        ReturnToMainMenuButton()
    {
        MainMenu.GetComponent<Canvas>().enabled = true;
        SettingsMenu.GetComponent<Canvas>().enabled = false;
    }

    void Update()
    {

    }

}