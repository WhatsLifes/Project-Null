using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Sanity sanity;
    [SerializeField] private FlashlightToggle flashlight;
    [SerializeField] private Inventory inventory;

    [SerializeField] private bool autoLoadOnStart = true;
    [SerializeField] private float loadDelay = 0.1f;

    private void Start()
    {
        StartCoroutine(InitLoad());
    }

    private IEnumerator InitLoad()
    {
        yield return null;

        if (autoLoadOnStart && GameManager.Instance != null)
        {
            LoadPlayerState();
        }
    }

    public void SavePlayerState()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SavePlayerState(player, sanity, flashlight, inventory);
        }
    }

    public void LoadPlayerState()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadPlayerState(player, sanity, flashlight, inventory);
        }
    }

    public void ChangeScene(string sceneName)
    {
        SavePlayerState();
        SceneManager.LoadScene(sceneName);
    }

    public void ChangeScene(int sceneIndex)
    {
        SavePlayerState();
        SceneManager.LoadScene(sceneIndex);
    }

    public void ChangeSceneWithFade(string sceneName, float fadeTime = 1f)
    {
        StartCoroutine(FadeRoutine(sceneName, fadeTime));
    }

    private IEnumerator FadeRoutine(string sceneName, float fadeTime)
    {
        SavePlayerState();
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene(sceneName);
    }
   
}