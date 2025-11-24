using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseCanvas;        // Whole pause canvas
    public GameObject mainMenuPanel;      // Panel with Resume/Options/Quit
    public GameObject optionsPanel;       // Panel with sliders

    [Header("Player")]
    [SerializeField] MonoBehaviour playerLookScript; // your camera look script

    [Header("Options UI")]
    public Slider sensitivitySlider;
    public Slider volumeSlider;

    [Header("Audio")]
    public AudioMixer audioMixer;          // Assign your MasterMixer here
    public string volumeParameter = "MasterVolume"; // must match exposed name

    public static bool IsPaused = false;

    // cache strongly-typed reference to look script
    private SimpleFPS _playerLook;        // change to your actual class name

    void Awake()
    {
        // Cast MonoBehaviour to your actual look script type
        _playerLook = playerLookScript as SimpleFPS;
        if (_playerLook == null)
        {
            Debug.LogWarning("PauseMenu: playerLookScript is not a PlayerLook.");
        }
    }

    void Start()
    {
        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        Time.timeScale = 1f;
        IsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize sliders from current settings
        if (_playerLook != null && sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 0.5f;
            sensitivitySlider.maxValue = 5f;
            sensitivitySlider.value = _playerLook.mouseSensitivity;
        }

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0.0001f; // avoid log(0)
            volumeSlider.maxValue = 1f;
            volumeSlider.value = 1f; // full volume by default
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        if (pauseCanvas != null)
            pauseCanvas.SetActive(true);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        Time.timeScale = 0f;
        IsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerLookScript != null)
            playerLookScript.enabled = false;
    }

    public void Resume()
    {
        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        Time.timeScale = 1f;
        IsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerLookScript != null)
            playerLookScript.enabled = true;
    }

    // Called by Options button
    public void OpenOptions()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    // Called by Back button in options
    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    // Hook this to SensitivitySlider OnValueChanged(float)
    public void OnSensitivityChanged(float newValue)
    {
        if (_playerLook != null)
        {
            _playerLook.mouseSensitivity = newValue;
        }
    }

    // Hook this to VolumeSlider OnValueChanged(float)
    public void OnVolumeChanged(float newValue)
    {
        if (audioMixer != null)
        {
            // Map [0,1] to decibels
            float dB = Mathf.Log10(newValue) * 20f;
            audioMixer.SetFloat(volumeParameter, dB);
        }
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
