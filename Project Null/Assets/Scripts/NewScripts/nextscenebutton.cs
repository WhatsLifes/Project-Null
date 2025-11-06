using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextSceneWithKey : MonoBehaviour
{
    [Header("Scene Settings")]
    public string nextSceneName = "Stage2";  // Name of the next scene
    public KeyCode loadKey = KeyCode.E;      // Key to press to load scene

    void Update()
    {
        if (Input.GetKeyDown(loadKey))
        {
            LoadNextScene();
        }
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
