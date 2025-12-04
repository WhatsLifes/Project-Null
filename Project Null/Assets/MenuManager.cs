using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingsMenu;

    private void Start()
    {
        if (mainMenu == null || settingsMenu == null)
        {
            Debug.LogError("Menu Manager: Missing references!");
            return;
        }

        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }

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
