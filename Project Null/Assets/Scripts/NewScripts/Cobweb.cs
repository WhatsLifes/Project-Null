using UnityEngine;

public class CobwebZone : MonoBehaviour
{
    public float slowMultiplier = 0.4f;
    public AudioSource cobwebSound;

    public float fadeSpeed = 6f;
    public float volumeBoost = 1.3f;

    private SimpleFPS player;
    private int playersInside;

    private float baseVolume;
    private float currentVolume;

    private float originalSpeedMultiplier;
    private bool isSlowed = false;

    void Start()
    {
        if (cobwebSound == null) return;

        baseVolume = cobwebSound.volume;
        currentVolume = 0f;

        cobwebSound.loop = true;
        cobwebSound.playOnAwake = false;
        cobwebSound.Stop();
    }

    void Update()
    {
        if (playersInside <= 0 || player == null || cobwebSound == null)
            return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        bool moving = (h != 0f || v != 0f);

        if (moving)
        {
            if (!cobwebSound.isPlaying)
                cobwebSound.Play();

            float target = baseVolume * volumeBoost;
            currentVolume = Mathf.MoveTowards(currentVolume, target, fadeSpeed * Time.deltaTime);
        }
        else
        {
            currentVolume = Mathf.MoveTowards(currentVolume, 0f, fadeSpeed * Time.deltaTime);

            if (currentVolume <= 0.01f && cobwebSound.isPlaying)
                cobwebSound.Stop();
        }

        cobwebSound.volume = currentVolume;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        SimpleFPS entering = other.GetComponent<SimpleFPS>();
        if (entering == null) return;

        player = entering;
        playersInside++;

        // Apply slow only once
        if (!isSlowed)
        {
            originalSpeedMultiplier = player.speedMultiplier;
            player.speedMultiplier = originalSpeedMultiplier * slowMultiplier;
            isSlowed = true;
        }

        Debug.Log("Entered cobweb");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        SimpleFPS exiting = other.GetComponent<SimpleFPS>();
        if (exiting == null) return;

        playersInside--;

        if (playersInside <= 0)
        {
            playersInside = 0;

            // Restore original speed safely
            if (isSlowed)
            {
                exiting.speedMultiplier = originalSpeedMultiplier;
                isSlowed = false;
            }

            player = null;
            currentVolume = 0f;

            if (cobwebSound != null)
            {
                cobwebSound.Stop();
                cobwebSound.time = 0f;
            }

            Debug.Log("Exited cobweb (audio forced stop)");
        }
    }
}