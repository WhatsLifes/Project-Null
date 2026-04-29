using Unity.VisualScripting.Dependencies.NCalc;
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
    [SerializeField] MonoBehaviour CamerabobScript;
    
    [Header("Options UI")]
    public Slider sensitivitySlider;
    public Slider ambienceVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider dialogueVolumeSlider;

    [Header("Audio")]
    public AudioMixer audioMixer;          // Assign your MasterMixer here
    public string ambienceVolumeParam = "AmbienceVolume";   // must match exposed name
    public string sfxVolumeParam = "SFXVolume";
    public string dialogueVolumeParam = "SpeakerFilter";

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

        SetupSlider(ambienceVolumeSlider, ambienceVolumeParam);
        SetupSlider(sfxVolumeSlider, sfxVolumeParam);
        SetupSlider(dialogueVolumeSlider, dialogueVolumeParam);

    }

    void SetupSlider(Slider slider, string param)
    {
        if (slider == null) return;
        
        slider.minValue = 0.0001f;
        slider.maxValue = 1f;

        float db;
        if (audioMixer.GetFloat(param, out db))
        {
            slider.value = Mathf.Pow(10f, db / 20f);
        }
        else
        {
            slider.value = 1f;
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
        if (CamerabobScript != null)
            CamerabobScript.enabled = false;
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
        if (CamerabobScript != null)
            CamerabobScript.enabled = true;
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

    // Hook these to each respective volume slider
    public void OnAmbienceVolumeChanged(float newValue)
    {
        SetVolume(ambienceVolumeParam, newValue);
    }
    public void OnSFXVolumeChanged(float newValue)
    {
        SetVolume(sfxVolumeParam, newValue);
    }
    public void OnDialogueVolumeChanged(float newValue)
    {
        SetVolume(dialogueVolumeParam, newValue);
    }

    void SetVolume(string param, float value)
    {
        if (audioMixer == null) return;
        float dB = Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(param, dB);
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
