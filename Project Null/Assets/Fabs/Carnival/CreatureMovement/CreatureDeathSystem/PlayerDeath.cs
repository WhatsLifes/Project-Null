using System.Collections;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [Header("Respawn")]
    public Transform spawnPoint;

    [Header("Player Components")]
    public GameObject playerModel;
    public MonoBehaviour movementScript;

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera deathCamera;

    [Header("Animation")]
    public Animator playerAnimator;
    public Animator creatureAnimator;
    public float Delay = 2f;
    public AudioSource deathAudio;
    public AudioClip deathClip;

    [Header("Animation State Names")]
    public string playerDeathState = "Player_Death";
    public string creatureKillState = "Creature_Kill";

    [Header("Fade")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;
    

    
    private bool isDead;

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("MonsterHitbox"))
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        movementScript.enabled = false;


        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // switch cameras
        if (mainCamera != null) mainCamera.enabled = false;
        if (deathCamera != null) deathCamera.enabled = true;
        
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        
        
        Debug.Log("Death sequence started");
        
        if (deathAudio != null && deathClip != null)
        {
            deathAudio.PlayOneShot(deathClip);
        }
        
        if (playerAnimator != null)
            playerAnimator.SetTrigger("Die");

        if (creatureAnimator != null)
            creatureAnimator.SetTrigger("Kill");

        playerModel.SetActive(false);

        yield return new WaitForSeconds(Delay*.7f);
        StartCoroutine(Fade(0, 1));
        yield return new WaitForSeconds(Delay*.3f);
        

        Respawn();
    }
    
    public void PlayDeathSound()
    {
        if (deathAudio != null && deathClip != null)
        {
            deathAudio.PlayOneShot(deathClip);
        }
    }
    
    void Respawn()
    {
        playerModel.SetActive(true);
        movementScript.enabled = true;
        
        StartCoroutine(Fade(1,0));
        
        if (mainCamera != null) mainCamera.enabled = true;
        if (deathCamera != null) deathCamera.enabled = false;



        isDead = false;
    }
    
    private IEnumerator Fade(float start, float end)
    {
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            if (fadeCanvas != null)
                fadeCanvas.alpha = Mathf.Lerp(start, end, t / fadeDuration);

            yield return null;
        }

        if (fadeCanvas != null)
            fadeCanvas.alpha = end;
    }

    
}