using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LifeManager : MonoBehaviour
{
    public static LifeManager Instance;

    [Header("Lives")]
    public int maxLives = 3;
    private int currentLives;

    [Header("Screen Fade")]
    public Image blackScreenImage;
    public float fadeDuration = 0.5f;

    [Header("Respawn")]
    public Transform respawnPoint;
    public GameObject player;

    [Header("References")]

    public static System.Action<int> OnLivesChanged;
}