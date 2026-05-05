using UnityEngine;
using UnityEngine.UI;

public class WakeUpSequence : MonoBehaviour
{
    [Header("UI References")]
    public Image fadeImage;
    public RectTransform topEyelid;
    public RectTransform bottomEyelid;
    public GameObject eyelidCanvas;

    [Header("Camera & Player References")]
    public Camera mainCamera;
    public Transform bedCameraPosition;
    public Transform playerCameraPosition;
    public Transform playerBody;
    public MeshRenderer[] playerMeshRenderers;
    public MonoBehaviour[] playerControlScripts;

    [Header("Blink Settings")]
    [Tooltip("Delay before character starts waking up")]
    public float initialWakeDelay = 1.5f;
    [Tooltip("How long it takes to open eyes (slow)")]
    public float blinkOpenDuration = 0.8f;
    [Tooltip("How long it takes to close eyes (faster)")]
    public float blinkCloseDuration = 0.4f;
    [Tooltip("How long eyes stay open between blinks")]
    public float eyesOpenHoldDuration = 0f;
    [Tooltip("Pause between blinks when eyes are closed")]
    public float pauseBetweenBlinks = 0.5f;

    [Header("Sit Up Settings")]
    [Tooltip("Time to sit up halfway")]
    public float sitUpHalfwayDuration = 1.5f;
    [Tooltip("Pause at halfway position")]
    public float halfwayPauseDuration = 0.3f;
    [Tooltip("Time to look left")]
    public float lookLeftDuration = 0.8f;
    [Tooltip("Pause while looking left")]
    public float lookLeftPauseDuration = 0.4f;
    [Tooltip("Time to look from left to right")]
    public float lookCenterToRightDuration = 1.2f;
    [Tooltip("Pause while looking right")]
    public float lookRightPauseDuration = 0.4f;
    [Tooltip("Time to return to center")]
    public float lookReturnCenterDuration = 0.6f;
    [Tooltip("How far to look left/right in degrees")]
    public float lookAngle = 25f;
    [Tooltip("Time to finish sitting up")]
    public float sitUpFinishDuration = 1.2f;

    [Header("Get Out of Bed Settings")]
    [Tooltip("Pause after sitting up fully")]
    public float sitUpPauseDuration = 0.5f;
    [Tooltip("Time to turn 90 degrees")]
    public float turnDuration = 1.5f;
    [Tooltip("Time to swing legs and stand")]
    public float standUpDuration = 1.5f;

    [Header("Camera Transition")]
    [Tooltip("Time to transition camera from bed to player")]
    public float cameraTransitionDuration = 1.5f;
    [Tooltip("Pause before enabling player control")]
    public float finalPauseDuration = 0.3f;

    [Header("Camera Angles")]
    [Tooltip("Camera tilt when lying down")]
    public float lyingDownCameraAngle = -85f;
    [Tooltip("Camera tilt when sitting halfway")]
    public float halfwaySitCameraAngle = -45f;
    [Tooltip("Camera tilt when fully sitting")]
    public float sittingCameraAngle = -20f;
    [Tooltip("Camera tilt when standing")]
    public float standingCameraAngle = 0f;

    [Header("Skip Settings")]
    [Tooltip("Key the player presses to skip the cutscene")]
    public KeyCode skipKey = KeyCode.Space;
    [Tooltip("Duration of the fade to black when skipping")]
    public float skipFadeDuration = 0.5f;
    [Tooltip("Optional UI text object showing the skip prompt (assign a Text or TMP_Text child of eyelidCanvas)")]
    public GameObject skipPromptObject;

    private float sequenceTimer = 0f;
    private bool sequenceComplete = false;
    private float initialPlayerRotation;
    private bool cameraTransitionStarted = false;
    private Quaternion cameraTransitionStartRotation;
    private bool phase1Logged = false;
    private bool phase2Logged = false;
    private bool phase3Logged = false;
    private bool isSkipping = false;
    private float skipFadeTimer = 0f;

    [SerializeField] private HUD hud;

    void Start()
    {
        foreach (var script in playerControlScripts)
        {
            if (script != null) script.enabled = false;
        }

        foreach (var renderer in playerMeshRenderers)
        {
            if (renderer != null) renderer.enabled = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (fadeImage != null)
        {
            fadeImage.color = Color.black;
            fadeImage.gameObject.SetActive(true);
        }

        SetEyelidPosition(0f);

        if (mainCamera != null && bedCameraPosition != null)
        {
            mainCamera.transform.SetParent(null);
            mainCamera.transform.position = bedCameraPosition.position;
            mainCamera.transform.rotation = bedCameraPosition.rotation;
            mainCamera.transform.rotation *= Quaternion.Euler(lyingDownCameraAngle, 0, 0);
        }

        if (playerBody != null)
        {
            initialPlayerRotation = playerBody.eulerAngles.y;
        }
    }

    void Update()
    {
        if (sequenceComplete) return;

        // Handle skip fade-out
        if (isSkipping)
        {
            skipFadeTimer += Time.deltaTime;
            float progress = skipFadeTimer / skipFadeDuration;

            if (fadeImage != null)
            {
                fadeImage.gameObject.SetActive(true);
                fadeImage.color = Color.Lerp(Color.clear, Color.black, progress);
            }

            if (progress >= 1f)
            {
                CompleteSequence();
            }
            return;
        }

        // Check for skip input
        if (Input.GetKeyDown(skipKey))
        {
            isSkipping = true;
            skipFadeTimer = 0f;

            // Snap player body to final rotation
            if (playerBody != null)
                playerBody.rotation = Quaternion.Euler(0, initialPlayerRotation + 90f, 0);

            // Snap camera to final position
            if (mainCamera != null && playerCameraPosition != null)
            {
                mainCamera.transform.position = playerCameraPosition.position;
                mainCamera.transform.rotation = playerCameraPosition.rotation;
            }

            // Hide skip prompt immediately
            if (skipPromptObject != null)
                skipPromptObject.SetActive(false);

            SetEyelidPosition(1f);
            return;
        }

        sequenceTimer += Time.deltaTime;
        RunSequence();
    }

    void RunSequence()
    {
        float t = sequenceTimer;

        // Phase 0: Initial wake delay
        if (t < initialWakeDelay)
        {
            return;
        }
        t -= initialWakeDelay;

        // Phase 1: Initial slow eye open
        if (t < blinkOpenDuration)
        {
            float progress = t / blinkOpenDuration;
            SetEyelidPosition(EaseOutCubic(progress));
            ClearFadeImage();
            if (progress > 0.99f && !phase1Logged)
            {
                Debug.Log($"[Phase 1 COMPLETE] at time {sequenceTimer:F2}, t value: {t:F2}");
                phase1Logged = true;
            }
            return;
        }
        t -= blinkOpenDuration;

        Debug.Log($"[After Phase 1] sequenceTimer: {sequenceTimer:F2}, t: {t:F4}");

        // Phase 1b: Hold eyes open briefly (if duration > 0)
        if (t < eyesOpenHoldDuration)
        {
            SetEyelidPosition(1f);
            Debug.Log($"[Phase 1b] Holding eyes open, t: {t:F4}");
            return;
        }
        t -= eyesOpenHoldDuration;

        Debug.Log($"[After Phase 1b] sequenceTimer: {sequenceTimer:F2}, t: {t:F4}");

        // Phase 2: First full blink (close and reopen)
        float firstBlinkDuration = blinkCloseDuration + pauseBetweenBlinks + blinkOpenDuration;
        if (t < firstBlinkDuration)
        {
            if (!phase2Logged)
            {
                Debug.Log($"[Phase 2 START] First blink starting at sequenceTimer: {sequenceTimer:F2}, t: {t:F4}");
                phase2Logged = true;
            }

            float blinkTimer = t;

            // Close eyes
            if (blinkTimer < blinkCloseDuration)
            {
                float progress = blinkTimer / blinkCloseDuration;
                float eyelidPos = 1f - progress;
                SetEyelidPosition(eyelidPos);
                if (blinkTimer < 0.05f)
                {
                    Debug.Log($"[Phase 2 - Closing] blinkTimer: {blinkTimer:F4}, progress: {progress:F4}, eyelidPos: {eyelidPos:F4}");
                }
                return;
            }
            blinkTimer -= blinkCloseDuration;

            // Pause while closed
            if (blinkTimer < pauseBetweenBlinks)
            {
                SetEyelidPosition(0f);
                return;
            }
            blinkTimer -= pauseBetweenBlinks;

            // Reopen
            if (blinkTimer < blinkOpenDuration)
            {
                float progress = blinkTimer / blinkOpenDuration;
                SetEyelidPosition(EaseOutCubic(progress));
                return;
            }
        }
        t -= firstBlinkDuration;

        if (!phase3Logged)
        {
            Debug.Log($"[Phase 3 START] Sitting up at sequenceTimer: {sequenceTimer:F2}, t: {t:F4}");
            phase3Logged = true;
        }

        // Phase 3: Sit up halfway
        if (t < sitUpHalfwayDuration)
        {
            float progress = t / sitUpHalfwayDuration;
            float angle = Mathf.Lerp(lyingDownCameraAngle, halfwaySitCameraAngle, EaseOutCubic(progress));
            SetCameraAngle(angle, 0f);
            SetEyelidPosition(1f);
            return;
        }
        t -= sitUpHalfwayDuration;

        // Phase 4: Second blink after sitting up
        float thirdBlinkDuration = blinkCloseDuration + blinkOpenDuration;
        if (t < thirdBlinkDuration)
        {
            if (t < blinkCloseDuration)
            {
                float progress = t / blinkCloseDuration;
                SetEyelidPosition(1f - EaseInCubic(progress));
                SetCameraAngle(halfwaySitCameraAngle, 0f);
                return;
            }
            else
            {
                float reopenTime = t - blinkCloseDuration;
                float progress = reopenTime / blinkOpenDuration;
                SetEyelidPosition(EaseOutCubic(progress));
                SetCameraAngle(halfwaySitCameraAngle, 0f);
                return;
            }
        }
        t -= thirdBlinkDuration;

        // Phase 5: Pause at halfway
        if (t < halfwayPauseDuration)
        {
            SetCameraAngle(halfwaySitCameraAngle, 0f);
            SetEyelidPosition(1f);
            return;
        }
        t -= halfwayPauseDuration;

        // Phase 6: Look left and right
        float totalLookDuration = lookLeftDuration + lookLeftPauseDuration + lookCenterToRightDuration + lookRightPauseDuration + lookReturnCenterDuration;
        if (t < totalLookDuration)
        {
            float lookTimer = t;

            if (lookTimer < lookLeftDuration)
            {
                float progress = lookTimer / lookLeftDuration;
                float yaw = Mathf.Lerp(0f, -lookAngle, EaseInOutCubic(progress));
                SetCameraAngle(halfwaySitCameraAngle, yaw);
                SetEyelidPosition(1f);
                return;
            }
            lookTimer -= lookLeftDuration;

            if (lookTimer < lookLeftPauseDuration)
            {
                SetCameraAngle(halfwaySitCameraAngle, -lookAngle);
                SetEyelidPosition(1f);
                return;
            }
            lookTimer -= lookLeftPauseDuration;

            if (lookTimer < lookCenterToRightDuration)
            {
                float progress = lookTimer / lookCenterToRightDuration;
                float yaw = Mathf.Lerp(-lookAngle, lookAngle, EaseInOutCubic(progress));
                SetCameraAngle(halfwaySitCameraAngle, yaw);
                SetEyelidPosition(1f);
                return;
            }
            lookTimer -= lookCenterToRightDuration;

            if (lookTimer < lookRightPauseDuration)
            {
                SetCameraAngle(halfwaySitCameraAngle, lookAngle);
                SetEyelidPosition(1f);
                return;
            }
            lookTimer -= lookRightPauseDuration;

            if (lookTimer < lookReturnCenterDuration)
            {
                float progress = lookTimer / lookReturnCenterDuration;
                float yaw = Mathf.Lerp(lookAngle, 0f, EaseInOutCubic(progress));
                SetCameraAngle(halfwaySitCameraAngle, yaw);
                SetEyelidPosition(1f);
                return;
            }
        }
        t -= totalLookDuration;
        SetCameraAngle(halfwaySitCameraAngle, 0f);

        // Phase 7: Finish sitting up
        if (t < sitUpFinishDuration)
        {
            float progress = t / sitUpFinishDuration;
            float angle = Mathf.Lerp(halfwaySitCameraAngle, sittingCameraAngle, EaseInOutCubic(progress));
            SetCameraAngle(angle, 0f);
            SetEyelidPosition(1f);
            return;
        }
        t -= sitUpFinishDuration;
        SetCameraAngle(sittingCameraAngle, 0f);

        // Phase 8: Pause while sitting
        if (t < sitUpPauseDuration)
        {
            SetEyelidPosition(1f);
            return;
        }
        t -= sitUpPauseDuration;

        // Phase 9: Turn 90 degrees
        if (t < turnDuration)
        {
            float progress = t / turnDuration;
            float targetRotation = initialPlayerRotation + 90f;
            float currentRotation = Mathf.LerpAngle(initialPlayerRotation, targetRotation, EaseInOutCubic(progress));

            if (playerBody != null)
            {
                playerBody.rotation = Quaternion.Euler(0, currentRotation, 0);
            }

            if (bedCameraPosition != null)
            {
                float rotationDelta = currentRotation - initialPlayerRotation;
                Quaternion baseRotation = bedCameraPosition.rotation * Quaternion.Euler(0, rotationDelta, 0);
                Quaternion pitchRotation = Quaternion.Euler(sittingCameraAngle, 0, 0);
                mainCamera.transform.rotation = baseRotation * pitchRotation;
            }

            SetEyelidPosition(1f);
            return;
        }
        t -= turnDuration;

        if (bedCameraPosition != null && playerBody != null)
        {
            float finalRotation = initialPlayerRotation + 90f;
            float rotationDelta = finalRotation - initialPlayerRotation;
            Quaternion baseRotation = bedCameraPosition.rotation * Quaternion.Euler(0, rotationDelta, 0);
            Quaternion pitchRotation = Quaternion.Euler(sittingCameraAngle, 0, 0);
            mainCamera.transform.rotation = baseRotation * pitchRotation;
        }

        // Phase 10: Stand up
        if (t < standUpDuration)
        {
            SetEyelidPosition(1f);
            return;
        }
        t -= standUpDuration;

        // Phase 11: Camera transition
        if (t < cameraTransitionDuration && playerCameraPosition != null)
        {
            float progress = t / cameraTransitionDuration;
            float smoothProgress = EaseInOutCubic(progress);

            if (!cameraTransitionStarted)
            {
                cameraTransitionStartRotation = mainCamera.transform.rotation;
                cameraTransitionStarted = true;
            }

            mainCamera.transform.position = Vector3.Lerp(
                bedCameraPosition.position,
                playerCameraPosition.position,
                smoothProgress
            );

            Quaternion targetRotation = playerCameraPosition.rotation;
            mainCamera.transform.rotation = Quaternion.Slerp(cameraTransitionStartRotation, targetRotation, smoothProgress);

            if (smoothProgress > 0.5f)
            {
                foreach (var renderer in playerMeshRenderers)
                {
                    if (renderer != null) renderer.enabled = true;
                }
            }

            SetEyelidPosition(1f);
            return;
        }
        t -= cameraTransitionDuration;

        // Phase 12: Final pause
        if (t < finalPauseDuration)
        {
            SetEyelidPosition(1f);
            return;
        }

        CompleteSequence();
    }

    void SetEyelidPosition(float normalized)
    {
        float screenHeight = Screen.height / 2f;

        if (topEyelid != null)
        {
            topEyelid.anchoredPosition = new Vector2(0, screenHeight * normalized);
        }

        if (bottomEyelid != null)
        {
            bottomEyelid.anchoredPosition = new Vector2(0, -screenHeight * normalized);
        }
    }

    void SetCameraAngle(float pitch, float yaw)
    {
        if (mainCamera != null && bedCameraPosition != null)
        {
            Quaternion baseRotation = bedCameraPosition.rotation;
            Quaternion pitchRotation = Quaternion.Euler(pitch, 0, 0);
            Quaternion yawRotation = Quaternion.Euler(0, yaw, 0);
            mainCamera.transform.rotation = baseRotation * pitchRotation * yawRotation;
        }
    }

    void ClearFadeImage()
    {
        if (fadeImage != null)
        {
            fadeImage.color = Color.clear;
        }
    }

    void CompleteSequence()
    {
        sequenceComplete = true;

        if (mainCamera != null && playerCameraPosition != null)
        {
            Quaternion currentWorldRotation = mainCamera.transform.rotation;
            mainCamera.transform.SetParent(playerCameraPosition);
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.rotation = currentWorldRotation;
        }

        foreach (var renderer in playerMeshRenderers)
        {
            if (renderer != null) renderer.enabled = true;
        }

        foreach (var script in playerControlScripts)
        {
            if (script != null) script.enabled = true;
        }

        if (eyelidCanvas != null)
        {
            eyelidCanvas.SetActive(false);
        }
        else
        {
            if (topEyelid != null) topEyelid.gameObject.SetActive(false);
            if (bottomEyelid != null) bottomEyelid.gameObject.SetActive(false);
        }
        if (fadeImage != null) fadeImage.gameObject.SetActive(false);

        hud.ShowHealthBar();
        hud.ShowSanityBar();
        hud.ShowInventorySlot();
        hud.ShowObjective1();
    }

    float EaseInCubic(float t)
    {
        return t * t * t;
    }

    float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }
}