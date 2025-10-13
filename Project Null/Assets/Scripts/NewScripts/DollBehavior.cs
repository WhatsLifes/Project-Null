using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DollBehavior : MonoBehaviour
{
    public enum DollType { Normal, Audio, Hostile }
    public DollType type = DollType.Normal;

    [Header("Audio")]
    public AudioClip scaryClip;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Health")]
    public int maxHealth = 3;
    private int health;

    [Header("AI")]
    public NavMeshAgent agent;
    private Transform player;

    private bool isDown = false;
    private bool isAttacking = false;
    private bool shouldBePickableAfterDeath = false;
    private bool shouldDisappearAfterDeath = false;
    private bool isInChair = false;

    private AudioSource audioSource;
    private Rigidbody rb;
    private Collider col;

    // Puzzle Eye ID
    public string leftEye;
    public string rightEye;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.playOnAwake = false;
        }

        health = maxHealth;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;

        if (agent != null)
            agent.enabled = false;
    }

    public void OnPickedUp()
    {
        if (type == DollType.Audio && scaryClip != null && audioSource != null)
            audioSource.PlayOneShot(scaryClip, volume);

        if (type == DollType.Hostile)
        {
            if (scaryClip != null && audioSource != null)
                audioSource.PlayOneShot(scaryClip, volume);

            ActivateAttackBehavior();
        }
    }

    public void OnPlaced()
    {
        isInChair = true;
        rb.isKinematic = true;
        rb.useGravity = false;

        if (agent != null)
            agent.enabled = false;

        isDown = false;
        isAttacking = false;
    }

    public void OnCorrectPlacement()
    {
        isInChair = true;
        rb.isKinematic = true;
        rb.useGravity = false;

        if (agent != null)
            agent.enabled = false;

        isDown = false;
        isAttacking = false;
    }

    public void OnIncorrectPlacement(bool isCorrectEyes)
    {
        isInChair = false;
        rb.isKinematic = false;
        rb.useGravity = true;

        // Pop off the chair safely
        rb.AddForce(Vector3.up * 3f + transform.forward * 1.2f, ForceMode.Impulse);

        // Decide what happens after death
        shouldBePickableAfterDeath = isCorrectEyes;
        shouldDisappearAfterDeath = !isCorrectEyes;

        // Start chasing after a delay — only enter "fight mode" once chasing begins
        StartCoroutine(DelayedAttackStart(0.5f));
    }

    private IEnumerator DelayedAttackStart(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (player != null && agent != null)
        {
            agent.enabled = true;
            agent.SetDestination(player.position);
            isAttacking = true; // ✅ officially enter fight mode
            Debug.Log($"{name} has entered fight mode!");
        }

        if (scaryClip != null && audioSource != null)
            audioSource.PlayOneShot(scaryClip, volume);
    }

    private void ActivateAttackBehavior()
    {
        isInChair = false;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (agent != null && player != null)
        {
            agent.enabled = true;
            agent.SetDestination(player.position);
            isAttacking = true; // ✅ start in fight mode immediately for Hostile dolls
            Debug.Log($"{name} (Hostile) started attacking!");
        }
    }

    public void TakeDamage(int damage)
    {
        // Allow damage only during fight/chase mode
        if (isDown)
        {
            Debug.Log($"{name} ignored hit: already down.");
            return;
        }

        if (isInChair)
        {
            Debug.Log($"{name} ignored hit: still in chair.");
            return;
        }

        if (!isAttacking)
        {
            Debug.Log($"{name} ignored hit: not in fight mode yet.");
            return;
        }

        // ✅ Now valid to take damage
        health -= damage;
        Debug.Log($"{name} took {damage} damage! Remaining: {health}");

        if (health <= 0)
            Down();
    }

    void Down()
    {
        isDown = true;
        isAttacking = false;

        if (agent != null)
            agent.enabled = false;

        rb.isKinematic = false;
        rb.useGravity = true;

        // Add a small random torque to make it tip naturally
        Vector3 torque = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
        rb.AddTorque(torque, ForceMode.Impulse);

        // Handle death outcomes
        if (shouldBePickableAfterDeath)
        {
            StartCoroutine(KeepAboveGround());
            ResetToPickable();
        }
        else
        {
            StartCoroutine(KeepAboveGround());
            StartCoroutine(DisappearAfterDelay(3f));
        }
    }

    private IEnumerator KeepAboveGround()
    {
        if (!rb.isKinematic)
        {
            rb.AddTorque(new Vector3(
                Random.Range(-5f, 5f),
                Random.Range(-5f, 5f),
                Random.Range(-5f, 5f)
            ), ForceMode.Impulse);
        }

        float timer = 0f;
        while (timer < 2f)
        {
            if (transform.position.y < 0.5f)
            {
                Vector3 pos = transform.position;
                pos.y = 0.5f;
                transform.position = pos;

                if (!rb.isKinematic)
                {
                    rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator DisappearAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public void ResetToPickable()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        col.isTrigger = false;
        transform.SetParent(null);

        isDown = false;
        isAttacking = false;
        isInChair = false;

        if (agent != null)
            agent.enabled = false;
    }

    void Update()
    {
        if (isAttacking && agent != null && player != null && agent.enabled)
        {
            agent.SetDestination(player.position);
        }
    }
}
