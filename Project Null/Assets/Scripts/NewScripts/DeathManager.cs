using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DeathManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas deathCanvas;
    [SerializeField] private CanvasGroup deathScreenGroup;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button quitToMenuButton;

    [Header("Player References (Auto-Find)")]
    [SerializeField] private Player player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private SimpleFPS playerController;
    [SerializeField] private HUD hud;

    [Header("Death Animation Settings")]
    [SerializeField] private float fallDuration = 2f;
    [SerializeField] private float fallRotationAmount = 90f;
    [SerializeField] private float dropHeight = 1.5f;
    [SerializeField] private float sidewaysDistance = 0.8f;
    [SerializeField] private float forwardDistance = 0.3f;
    [SerializeField] private AnimationCurve fallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("UI Settings")]
    [SerializeField] private float uiDelayAfterFall = 0.5f;
    [SerializeField] private float uiFadeDuration = 1f;

    [Header("Scene Settings")]
    [SerializeField] private string menuSceneName = "MainMenu";

    private bool isDead = false;

    private void Awake()
    {
        // Auto-find player components if not assigned
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }

        if (player != null)
        {
            if (playerCamera == null)
            {
                playerCamera = player.GetComponentInChildren<Camera>();
            }

            if (playerController == null)
            {
                playerController = player.GetComponent<SimpleFPS>();
            }
        }

        // Auto-find HUD if not assigned
        if (hud == null)
        {
            hud = FindObjectOfType<HUD>();
        }
    }

    private void Start()
    {
        // Hide death screen at start
        if (deathScreenGroup != null)
        {
            deathScreenGroup.alpha = 0f;
            deathScreenGroup.gameObject.SetActive(false);
            deathScreenGroup.blocksRaycasts = false;
        }

        // Set up buttons
        if (tryAgainButton != null)
            tryAgainButton.onClick.AddListener(TryAgain);

        if (quitToMenuButton != null)
            quitToMenuButton.onClick.AddListener(QuitToMenu);
    }

    private void OnEnable()
    {
        if (player != null)
            player.OnDeath += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnDeath -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        if (isDead) return;

        isDead = true;
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // Disable player controls
        if (playerController != null)
            playerController.enabled = false;

        // Camera fall animation - realistic fall with pivot at feet
        if (playerCamera != null)
        {
            float elapsed = 0f;

            // Store starting values
            Quaternion startRotation = playerCamera.transform.localRotation;
            Vector3 startPosition = playerCamera.transform.localPosition;

            // Target rotation
            Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, fallRotationAmount);

            // Target position - moves in an arc as if pivoting from feet
            float sidewaysDirection = Mathf.Sign(fallRotationAmount);
            Vector3 offset = new Vector3(
                sidewaysDirection * sidewaysDistance,
                -dropHeight,
                forwardDistance
            );
            Vector3 targetPosition = startPosition + offset;

            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fallDuration;
                float curveT = fallCurve.Evaluate(t);

                // Rotate AND move
                playerCamera.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, curveT);
                playerCamera.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, curveT);

                yield return null;
            }

            playerCamera.transform.localRotation = targetRotation;
            playerCamera.transform.localPosition = targetPosition;
        }

        // Wait before showing UI
        yield return new WaitForSeconds(uiDelayAfterFall);

        // Hide the HUD and disable the HUD script to stop updates
        if (hud != null)
        {
            hud.HideAllHUD();
            hud.enabled = false; // NEW: Disable the script to stop Update loop
        }

        // Show death screen UI
        if (deathScreenGroup != null)
        {
            deathScreenGroup.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < uiFadeDuration)
            {
                elapsed += Time.deltaTime;
                deathScreenGroup.alpha = Mathf.Clamp01(elapsed / uiFadeDuration);
                yield return null;
            }

            deathScreenGroup.alpha = 1f;
            deathScreenGroup.blocksRaycasts = true;

            // Show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void TryAgain()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(menuSceneName);
    }
}