using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu References")]
    public CanvasGroup mainMenu;
    public CanvasGroup settingsMenu;

    public void PlayGame()
    {
        SceneManager.LoadScene("Stage1");
    }

    public void OpenSettings()
    {
        Show(settingsMenu);
        Hide(mainMenu);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    void Show(CanvasGroup cg)
    {
        cg.alpha = 1;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    void Hide(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
    void Start()
    {
        Show(mainMenu);
        Hide(settingsMenu);
    }
}