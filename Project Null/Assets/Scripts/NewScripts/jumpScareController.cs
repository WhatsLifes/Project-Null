using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JumpscareController : MonoBehaviour
{
    [Header("Scare Settings")]
    public float lookSpeed = 10f;
    public float freezeDuration = 0.5f;
    public float despawnDelay = 0.5f;
    public AudioClip scareSound;
    public AudioSource audioSource;

    [Header("Visual Effect (optional)")]
    public Image visualEffect;
    public float visualFadeIn = 0.2f;
    public float visualFadeOut = 0.2f;
    [Range(0f, 1f)] public float visualMaxAlpha = 0.5f;

    [Header("Camera Zoom Effect")]
    public float zoomedFOV = 35f;
    public float zoomInDuration = 1f;
    public float zoomOutDuration = 1f;
    public float lookTargetHeight = 1.5f;

    private Renderer[] renderers;
    private bool isScaring = false;
    private bool fovLocked = false;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        SetVisible(false);

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void BeginScare(GameObject playerObj, AudioClip soundClip, float duration)
    {
        if (isScaring) return;
        isScaring = true;

        if (soundClip != null)
            scareSound = soundClip;
        if (duration > 0f)
            freezeDuration = duration;

        StartCoroutine(ScareSequence(playerObj));
    }

    private IEnumerator ScareSequence(GameObject playerObj)
    {
        SetVisible(true);

        if (scareSound != null)
            audioSource.PlayOneShot(scareSound);

        Transform playerT = playerObj.transform;
        Camera cam = playerObj.GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;

        SimpleFPS fps = playerObj.GetComponent<SimpleFPS>();
        MonoBehaviour otherMove = null;
        if (fps == null)
            otherMove = playerObj.GetComponent<MonoBehaviour>();

        // 🔹 Disable movement
        if (fps != null)
            fps.enabled = false;
        else if (otherMove != null)
            otherMove.enabled = false;

        // 🔹 Smoothly rotate camera toward prefab
        Quaternion finalLookRot = cam.transform.rotation;
        if (cam != null)
        {
            Vector3 lookPos = transform.position + Vector3.up * lookTargetHeight;
            Quaternion startRot = cam.transform.rotation;
            Quaternion targetRot = Quaternion.LookRotation(lookPos - cam.transform.position);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * (lookSpeed * 0.5f);
                cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            cam.transform.rotation = targetRot;
            finalLookRot = targetRot; // save this to apply later
        }

        // 🔹 Fade in overlay
        if (visualEffect != null)
            yield return StartCoroutine(FadeOverlay(0f, visualMaxAlpha, visualFadeIn));

        // 🔹 Lock FOV
        fovLocked = true;
        float originalFOV = cam.fieldOfView;

        // 🔹 Smooth zoom-in
        yield return StartCoroutine(CameraZoom(cam, originalFOV, zoomedFOV, zoomInDuration));

        // 🔹 Hold zoomed in
        yield return new WaitForSeconds(freezeDuration);

        // 🔹 Fade out overlay
        if (visualEffect != null)
            yield return StartCoroutine(FadeOverlay(visualMaxAlpha, 0f, visualFadeOut));

        // 🔹 Smooth zoom-out
        yield return StartCoroutine(CameraZoom(cam, cam.fieldOfView, originalFOV, zoomOutDuration));

        fovLocked = false;

        // ✅ Keep the player facing the same direction as the camera
        Vector3 flatForward = cam.transform.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude > 0.001f)
            playerT.rotation = Quaternion.LookRotation(flatForward);

        // 🔹 Restore controls
        if (fps != null)
            fps.enabled = true;
        else if (otherMove != null)
            otherMove.enabled = true;

        SetVisible(false);
        Destroy(gameObject, despawnDelay);
        isScaring = false;
    }

    private IEnumerator FadeOverlay(float from, float to, float time)
    {
        if (visualEffect == null)
            yield break;

        visualEffect.gameObject.SetActive(true);
        Color c = visualEffect.color;
        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / time));
            c.a = Mathf.Lerp(from, to, f);
            visualEffect.color = c;
            yield return null;
        }

        c.a = to;
        visualEffect.color = c;

        if (Mathf.Approximately(to, 0f))
            visualEffect.gameObject.SetActive(false);
    }

    private IEnumerator CameraZoom(Camera cam, float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            cam.fieldOfView = Mathf.Lerp(from, to, t);
            yield return null;
        }

        cam.fieldOfView = to;
    }

    private void SetVisible(bool visible)
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);

        foreach (var r in renderers)
            if (r != null)
                r.enabled = visible;
    }
}
