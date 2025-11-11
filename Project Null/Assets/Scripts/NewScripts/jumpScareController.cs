using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JumpscareController : MonoBehaviour
{
    [Header("Scare Settings")]
    public float lookSpeed = 6f;
    public float freezeDuration = 2f;
    public float despawnDelay = 0.5f;
    public AudioClip scareSound;
    public AudioSource audioSource;

    [Header("Visual Effect (optional)")]
    public Image visualEffect;
    public float visualFadeIn = 0.25f;
    public float visualFadeOut = 0.5f;
    [Range(0f, 1f)] public float visualMaxAlpha = 0.8f;

    [Header("Camera Zoom Effect")]
    public float zoomedFOV = 35f;      // how zoomed in it gets
    public float zoomInSpeed = 3f;     // how fast it zooms in
    public float zoomOutSpeed = 2f;    // how fast it zooms out

    public float lookTargetHeight = 1.5f;

    private Renderer[] renderers;
    private bool isScaring = false;

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

        // 🔹 Get the player camera safely (even if it’s not tagged)
        Camera cam = playerObj.GetComponentInChildren<Camera>();
        if (cam == null)
            cam = Camera.main;

        SimpleFPS fps = playerObj.GetComponent<SimpleFPS>();
        MonoBehaviour otherMove = null;

        if (fps == null)
            otherMove = playerObj.GetComponent<MonoBehaviour>();

        // Force player to look at prefab
        if (fps != null)
            fps.ForceLookAt(transform.position + Vector3.up * lookTargetHeight);
        else
        {
            if (otherMove != null) otherMove.enabled = false;

            if (cam != null)
            {
                Quaternion startRot = cam.transform.rotation;
                Vector3 lookPos = transform.position + Vector3.up * lookTargetHeight;
                Quaternion targetRot = Quaternion.LookRotation(lookPos - cam.transform.position);

                float elapsed = 0f;
                float duration = 1f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime * lookSpeed;
                    cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
                    yield return null;
                }
                cam.transform.rotation = targetRot;
            }
        }

        // 🔹 Begin visual overlay
        if (visualEffect != null)
            yield return StartCoroutine(FadeOverlay(0f, visualMaxAlpha, visualFadeIn));

        // 🔹 Start FOV zoom (this now ALWAYS works)
        if (cam != null)
            yield return StartCoroutine(CameraZoom(cam, cam.fieldOfView, zoomedFOV, zoomInSpeed));

        yield return new WaitForSeconds(freezeDuration);

        // 🔹 Fade out overlay
        if (visualEffect != null)
            yield return StartCoroutine(FadeOverlay(visualMaxAlpha, 0f, visualFadeOut));

        // 🔹 Zoom back out
        if (cam != null)
            yield return StartCoroutine(CameraZoom(cam, cam.fieldOfView, 60f, zoomOutSpeed));

        // Release control
        if (fps != null)
            fps.ReleaseLook();
        else
        {
            if (cam != null && otherMove != null)
            {
                Vector3 camForward = cam.transform.forward;
                camForward.y = 0f;
                if (camForward.sqrMagnitude > 0.001f)
                    playerT.rotation = Quaternion.LookRotation(camForward);
            }

            if (otherMove != null)
                otherMove.enabled = true;
        }

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

    private IEnumerator CameraZoom(Camera cam, float from, float to, float speed)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            cam.fieldOfView = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
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
