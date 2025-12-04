using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject mainMenu;
    public GameObject settingsMenu;

    public void PlayGame()
    {
        SceneManager.LoadScene("Stage1");
    }

    public void OpenSettings()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
