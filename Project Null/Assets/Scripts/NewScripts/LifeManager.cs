using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LifeManager : MonoBehaviour
{
    public static LifeManager Instance;

    [Header("Lives")]
    public int maxLives = 3;
    private int currentLives;

    [Header("Screen Fade")]
    public Image blackScreenImage;
    public float fadeDuration = 0.5f;

    [Header("Respawn")]
    public Transform respawnPoint;
    public GameObject player;

    [Header("References")]
    public MonitorController monitorController;

    public static System.Action<int> OnLivesChanged;

    private bool isDying = false;

    private void Awake()
    {
        Instance = this;
        currentLives = maxLives;
    }

    private void Start()
    {
        SetBlackScreen(0f);
        OnLivesChanged?.Invoke(currentLives);
    }

    public void PlayerDied()
    {
        if (isDying) return;
        isDying = true;

        currentLives--;
        OnLivesChanged?.Invoke(currentLives);

        if (currentLives <= 0)
        {
            StartCoroutine(FadeAndRestart());
        }
        else
        {
            StartCoroutine(FadeAndRespawn());
        }
    }

    private IEnumerator FadeAndRespawn()
    {
        yield return StartCoroutine(FadeTo(1f));
        yield return new WaitForSeconds(0.5f);

        player.transform.position = respawnPoint.position;
        player.transform.position = respawnPoint.position;

        yield return StartCoroutine(FadeTo(0f));
        isDying = false;
    }

    private IEnumerator FadeAndRestart()
    {
        yield return StartCoroutine(FadeTo(1f));
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = blackScreenImage.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            SetBlackScreen(a);
            yield return null;
        }

        SetBlackScreen(targetAlpha);
    }

    private void SetBlackScreen(float alpha)
    {
        Color c = blackScreenImage.color;
        c.a = alpha;
        blackScreenImage.color = c;
    }
}