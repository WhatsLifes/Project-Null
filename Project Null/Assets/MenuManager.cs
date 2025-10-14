using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingsMenu;

    public void OpenSettings()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void BackToMain()
    {
        settingsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
}


