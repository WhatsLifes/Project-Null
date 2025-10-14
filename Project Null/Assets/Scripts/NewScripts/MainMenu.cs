using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Called when the Play button is pressed
    public void PlayGame()
    {
        // Load the scene named "Stage1"
        SceneManager.LoadScene("Stage1");
    }

    // Optional: Quit game
    public void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }
}