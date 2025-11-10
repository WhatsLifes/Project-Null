using UnityEngine;
using System.Collections;

public class JumpscareController : MonoBehaviour
{
    [Header("Effects")]
    public float appearDistance = 2f;
    public AudioSource audioSource;
    public GameObject visualEffect;
    public float fadeOutTime = 0.5f;

    private Renderer[] renderers;
    private bool active = false;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        SetVisible(false);
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void BeginScare(GameObject player, AudioClip sound, float duration)
    {
        if (active) return;
        active = true;

        StartCoroutine(ScareSequence(player, sound, duration));
    }

    private IEnumerator ScareSequence(GameObject player, AudioClip sound, float duration)
    {
        SetVisible(true);

        // ✅ Use SimpleFPS control system instead of disabling component
        var controller = player.GetComponent<SimpleFPS>();
        if (controller != null)
            controller.ForceLookAt(transform.position);

        // play sound
        if (sound != null)
        {
            audioSource.clip = sound;
            audioSource.Play();
        }

        // visual overlay
        if (visualEffect != null)
            visualEffect.SetActive(true);

        yield return new WaitForSeconds(duration);

        if (visualEffect != null)
            visualEffect.SetActive(false);

        SetVisible(false);

        // ✅ Release control
        if (controller != null)
            controller.ReleaseLook();

        Destroy(gameObject, fadeOutTime);
    }

    private void SetVisible(bool visible)
    {
        foreach (var r in renderers)
            r.enabled = visible;
    }
}

