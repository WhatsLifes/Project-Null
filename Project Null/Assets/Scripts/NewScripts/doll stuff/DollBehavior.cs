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

    [Header("AI Reference")]
    public Enemy enemyScript; // Reference to Enemy script

    [Header("Puzzle")]
    public DollPuzzleManager.EyeType leftEye;
    public DollPuzzleManager.EyeType rightEye;

    private bool isInChair = false;
    private bool shouldBePickableAfterDeath = false;
    private bool shouldDisappearAfterDeath = false;

    private AudioSource audioSource;
    private Rigidbody rb;
    private Collider col;

    private Animator anim;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        anim = GetComponentInChildren<Animator>();


        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.playOnAwake = false;
        }

        // Get Enemy script reference
        if (enemyScript == null)
            enemyScript = GetComponent<Enemy>();

        // Make sure Enemy starts deactivated
        if (enemyScript != null)
        {
            enemyScript.Deactivate();
            enemyScript.canBeKilled = false;
        }

        if (enemyScript == null)
        {
            Debug.LogError($"{gameObject.name} - Enemy script NOT FOUND!");
        }
        else
        {
            Debug.Log($"{gameObject.name} - Enemy script found and assigned");
        }
    }

    public void OnPickedUp()
    {
        Debug.Log($"{gameObject.name} - OnPickedUp called, type: {type}");

        if (type == DollType.Audio && scaryClip != null && audioSource != null)
            audioSource.PlayOneShot(scaryClip, volume);

        if (type == DollType.Hostile)
        {
            if (scaryClip != null && audioSource != null)
                audioSource.PlayOneShot(scaryClip, volume);

            // IMMEDIATELY force drop from player's hands and activate
            ForceDropAndActivate();

            shouldBePickableAfterDeath = true;
            shouldDisappearAfterDeath = false;
        }
    }

    private void ForceDropAndActivate()
    {
        Debug.Log($"{gameObject.name} - Forcing drop and activating hostile behavior");

        // Unparent from player immediately (in case pickup system parents it)
        transform.SetParent(null);

        // Enable physics immediately
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero; // Reset any weird velocity
            rb.angularVelocity = Vector3.zero;
        }

        // Make collider solid (not trigger)
        if (col != null)
        {
            col.isTrigger = false;
        }

        // Small delay to let it fall away from player, then activate AI
        StartCoroutine(ActivateAfterForceDrop());
    }

    private IEnumerator ActivateAfterForceDrop()
    {
        // Wait a tiny bit for physics to move it away from player
        yield return new WaitForSeconds(0.2f);

        Debug.Log($"{gameObject.name} - Activating attack behavior after force drop");
        ActivateAttackBehavior();
    }

    public void OnPlaced()
    {
        isInChair = true;
        rb.isKinematic = true;
        rb.useGravity = false;

        // Deactivate Enemy AI when placed in chair
        if (enemyScript != null)
        {
            enemyScript.Deactivate();
            enemyScript.canBeKilled = false; // Protect from damage in chair
        }
    }

    public void OnCorrectPlacement()
    {
        isInChair = true;
        rb.isKinematic = true;
        rb.useGravity = false;

        // Deactivate Enemy AI
        if (enemyScript != null)
        {
            enemyScript.Deactivate();
            enemyScript.canBeKilled = false;
        }
    }

    public void OnIncorrectPlacement(bool isCorrectEyes)
    {
        isInChair = false;
        rb.isKinematic = false;
        rb.useGravity = true;

        // Pop off the chair
        rb.AddForce(Vector3.up * 3f + transform.forward * 1.2f, ForceMode.Impulse);

        // Start chasing after a delay
        StartCoroutine(DelayedAttackStart(0.5f));

        // Decide what happens after death
        shouldBePickableAfterDeath = isCorrectEyes;
        shouldDisappearAfterDeath = !isCorrectEyes;
    }

    private IEnumerator DelayedAttackStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return new WaitForSeconds(0.8f);

        ActivateAttackBehavior();

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

        gameObject.layer = 8;
        Debug.Log($"{gameObject.name} - Layer changed to Enemy (unpickable while hostile)");

        // Activate Enemy script
        if (enemyScript != null)
        {
            enemyScript.Activate();
            enemyScript.canBeKilled = true;
        }

        if (anim != null) 
        {
            anim.SetBool("isMoving", true);
        }
    }

    // Called by Enemy script when health reaches 0
    public void Down()  //  Changed from OnEnemyDeath() to match Enemy script's call
    {
        // Make Enemy do ragdoll death
        if (enemyScript != null)
        {
            enemyScript.RagdollDeath();
        }

        if (type == DollType.Hostile)
        {
            type = DollType.Normal;
        }

        gameObject.layer = 11;
        Debug.Log($"{gameObject.name} - Layer set to interactable");


        // Handle puzzle outcomes
        if (shouldBePickableAfterDeath)
        {
            StartCoroutine(KeepAboveGround());
            ResetToPickable();
        }
        else
        {
            StartCoroutine(KeepAboveGround());
            StartCoroutine(DisappearAfterDelay(1.5f));
        }
    }

    private IEnumerator KeepAboveGround()
    {
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
        gameObject.layer = 11;
        Debug.Log($"{gameObject.name} - Layer set to pickupable");

        rb.isKinematic = false;
        rb.useGravity = true;
        col.isTrigger = false;
        transform.SetParent(null);

        isInChair = false;

        // Deactivate Enemy
        if (enemyScript != null)
            enemyScript.Deactivate();
    }
}