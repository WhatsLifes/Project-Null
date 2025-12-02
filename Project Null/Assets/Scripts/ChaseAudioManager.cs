using UnityEngine;
using System.Collections.Generic;

public class ChaseAudioManager : MonoBehaviour
{
    public static ChaseAudioManager Instance;
    
    public AudioSource audioSource;
    public AudioClip chaseClip;

    private HashSet<MannequinEnemy> chasingEnemies = new HashSet<MannequinEnemy>();
    private bool isPlaying = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void EnemeyStartedChasing(MannequinEnemy enemy)
    {
        chasingEnemies.Add(enemy);
        if (!isPlaying)
        {
            PlayChaseAudio();
            isPlaying = true;
        }
    }

    public void EnemyStoppedChasing(MannequinEnemy enemy)
    {
        chasingEnemies.Remove(enemy);
        if (chasingEnemies.Count == 0)
        {
            isPlaying = false;
        }
    }

    private void PlayChaseAudio()
    {
        if (audioSource != null && chaseClip != null)
        {
            audioSource.PlayOneShot(chaseClip);
        }
    }
}
