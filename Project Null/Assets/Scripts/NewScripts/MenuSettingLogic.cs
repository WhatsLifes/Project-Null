using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Player Look (Optional)")]
    [SerializeField] private SimpleFPS playerLook;

    [Header("Sliders")]
    public Slider sensitivitySlider;
    public Slider ambienceVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider dialogueVolumeSlider;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    public string ambienceVolumeParam = "AmbienceVolume";
    public string sfxVolumeParam = "SFXVolume";
    public string dialogueVolumeParam = "SpeakerFilter";

    private void Start()
    {
        // Sensitivity
        if (playerLook != null && sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 0.5f;
            sensitivitySlider.maxValue = 5f;
            sensitivitySlider.value = playerLook.mouseSensitivity;
        }

        // Audio sliders
        SetupSlider(ambienceVolumeSlider, ambienceVolumeParam);
        SetupSlider(sfxVolumeSlider, sfxVolumeParam);
        SetupSlider(dialogueVolumeSlider, dialogueVolumeParam);
    }

    private void SetupSlider(Slider slider, string param)
    {
        if (slider == null || audioMixer == null) return;

        slider.minValue = 0.0001f;
        slider.maxValue = 1f;

        if (audioMixer.GetFloat(param, out float db))
            slider.value = Mathf.Pow(10f, db / 20f);
        else
            slider.value = 1f;
    }

    public void OnSensitivityChanged(float newValue)
    {
        if (playerLook != null)
            playerLook.mouseSensitivity = newValue;
    }

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

    private void SetVolume(string param, float value)
    {
        if (audioMixer == null) return;

        float db = Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(param, db);
    }
}
