using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private Sanity sanity;
    [SerializeField] private FlashlightToggle flashlight;
    [SerializeField] private Inventory inventory;

    [Header("Auto-Load Settings")]
    [Tooltip("Automatically load saved state when this scene starts")]
    [SerializeField] private bool autoLoadOnStart = true;
    [Tooltip("Delay before loading state (to let scene initialize)")]
    [SerializeField] private float loadDelay = 0.1f;

    private void Start()
    {
        if (autoLoadOnStart && GameManager.Instance != null)
        {
            StartCoroutine(LoadStateAfterDelay());
        }
    }

    private IEnumerator LoadStateAfterDelay()
    {
        yield return new WaitForSeconds(loadDelay);
        LoadPlayerState();
    }

    // Call this before changing scenes
    public void SavePlayerState()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SavePlayerState(player, sanity, flashlight, inventory);
        }
        else
        {
            Debug.LogWarning("GameManager not found! Make sure it exists in the scene.");
        }
    }

    // Call this after loading a new scene
    public void LoadPlayerState()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadPlayerState(player, sanity, flashlight, inventory);
        }
        else
        {
            Debug.LogWarning("GameManager not found! Make sure it exists in the scene.");
        }
    }

    // Helper method to change scenes with automatic save
    public void ChangeScene(string sceneName)
    {
        SavePlayerState();
        SceneManager.LoadScene(sceneName);
    }

    // Helper method to change scenes with automatic save (by build index)
    public void ChangeScene(int sceneIndex)
    {
        SavePlayerState();
        SceneManager.LoadScene(sceneIndex);
    }

    // Optional: Add fade transition
    public void ChangeSceneWithFade(string sceneName, float fadeTime = 1f)
    {
        StartCoroutine(FadeAndChangeScene(sceneName, fadeTime));
    }

    private IEnumerator FadeAndChangeScene(string sceneName, float fadeTime)
    {
        // You can add fade-to-black effect here if you have a fade canvas
        SavePlayerState();
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene(sceneName);
    }
}