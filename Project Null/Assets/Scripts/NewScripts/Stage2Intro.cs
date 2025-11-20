using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Stage2Intro : MonoBehaviour
{
    [Header("UI (Eyelids + Fade)")]
    [SerializeField] private RectTransform topEyelid;
    [SerializeField] private RectTransform bottomEyelid;
    [SerializeField] private GameObject eyelidCanvas;   // The canvas holding the eyelids
    [SerializeField] private Image fadeImage;           // Optional full-screen black Image

    [Header("Timing")]
    [Tooltip("Delay before the eye begins opening")]
    [SerializeField] private float initialDelay = 0.8f;
    [Tooltip("How long the eye-opening takes")]
    [SerializeField] private float openDuration = 1.0f;
    [Tooltip("Optional delay after opening before enabling controls/HUD")]
    [SerializeField] private float postOpenDelay = 0.15f;

    [Header("Gameplay Control")]
    [SerializeField] private MonoBehaviour[] playerControlScripts; // movement/look/scripts to disable during intro

    [Header("HUD")]
    [SerializeField] private HUD hud;                    // Your HUD script

    private float timer;
    private bool finished;

    private void Start()
    {
        // Disable gameplay scripts
        if (playerControlScripts != null)
            foreach (var s in playerControlScripts)
                if (s) s.enabled = false;

        // Lock and hide cursor (optional for FPS)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Eyelids start closed
        SetEyelidPosition(0f);

        // Ensure eyelid canvas is on
        if (eyelidCanvas) eyelidCanvas.SetActive(true);

        // Optional fade to black starts fully opaque
        if (fadeImage)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = Color.black;
        }
    }

    private void Update()
    {
        if (finished) return;

        timer += Time.deltaTime;

        // Wait the initial delay
        if (timer < initialDelay)
            return;

        float t = Mathf.Clamp01((timer - initialDelay) / openDuration);

        // Animate lids open with an ease-out feel
        float eased = EaseOutCubic(t);
        SetEyelidPosition(eased);

        // Fade the black image away in sync (optional)
        if (fadeImage)
            fadeImage.color = Color.Lerp(Color.black, Color.clear, eased);

        if (t >= 1f)
        {
            // Once fully open, finish up after a tiny buffer
            StartCoroutine(FinishAfterDelay(postOpenDelay));
            finished = true; // prevent re-entering
        }
    }

    private IEnumerator FinishAfterDelay(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        // Hide eyelids & fade
        if (eyelidCanvas) eyelidCanvas.SetActive(false);
        if (fadeImage) fadeImage.gameObject.SetActive(false);

        // Re-enable gameplay
        if (playerControlScripts != null)
            foreach (var s in playerControlScripts)
                if (s) s.enabled = true;

        // Bring up the HUD with proper fade-ins
        if (hud != null)
        {
            hud.ShowHealthBar();
            hud.ShowSanityBar();
            hud.ShowBatteryDisplay();      // Now uses the fade-in function!
            hud.ShowInventoryDisplay();    // Now uses the fade-in function!
            hud.ShowObjective6();
        }
    }

    /// <summary>
    /// normalized: 0 = fully closed, 1 = fully open
    /// Moves eyelids in canvas space so it scales with resolution/Canvas.
    /// </summary>
    private void SetEyelidPosition(float normalized)
    {
        float halfHeight = Screen.height * 0.5f;

        // Use parent rect height if available (better for Canvas scaling)
        RectTransform root = null;
        if (topEyelid) root = (RectTransform)topEyelid.parent;
        if (!root && bottomEyelid) root = (RectTransform)bottomEyelid.parent;
        if (root) halfHeight = root.rect.height * 0.5f;

        if (topEyelid)
            topEyelid.anchoredPosition = new Vector2(0f, halfHeight * normalized);

        if (bottomEyelid)
            bottomEyelid.anchoredPosition = new Vector2(0f, -halfHeight * normalized);
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}