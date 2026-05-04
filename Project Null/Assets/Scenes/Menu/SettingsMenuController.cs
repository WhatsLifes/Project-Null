using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup mainMenu;
    public CanvasGroup settingsMenu;

    [Header("Player Look")]  
    public SimpleFPS playerLook;  

    [Header("Sliders")]  
    public Slider sensitivitySlider;  
    public Slider musicSlider;  
    public Slider sfxSlider;  
    public Slider dialogueSlider;  

    [Header("Audio Mixer")]  
    public AudioMixer audioMixer;  
    public string musicParam = "MusicVolume";  
    public string sfxParam = "SFXVolume";  
    public string dialogueParam = "DialogueVolume";  

    private void Start()  
    {  
        LoadInitialValues();  
    }  

    private void LoadInitialValues()  
    {  
        if (playerLook != null)  
        {  
            sensitivitySlider.minValue = 0.5f;  
            sensitivitySlider.maxValue = 5f;  
            sensitivitySlider.value = playerLook.mouseSensitivity;  
        }  

        SetupSlider(musicSlider, musicParam);  
        SetupSlider(sfxSlider, sfxParam);  
        SetupSlider(dialogueSlider, dialogueParam);  
    } 

    private void SetupSlider(Slider slider, string param)
    {
        if (audioMixer.GetFloat(param, out float db))
            slider.value = Mathf.Pow(10f, db / 20f);
        else
            slider.value = 1f;

        SetVolume(param, slider.value);

        slider.onValueChanged.AddListener(v => SetVolume(param, v));  
    }

    public void OnSensitivityChanged(float v)  
    {  
        if (playerLook != null)  
            playerLook.mouseSensitivity = v;  
    }  

    public void OnMusicChanged(float v) => SetVolume(musicParam, v);  
    public void OnSFXChanged(float v) => SetVolume(sfxParam, v);  
    public void OnDialogueChanged(float v) => SetVolume(dialogueParam, v);  

    private void SetVolume(string param, float v)  
    {  
        v = Mathf.Clamp(v, 0.0001f, 1f);  
        float db = Mathf.Log10(v) * 20f;  
        audioMixer.SetFloat(param, db);  
    }  

    public void BackToMainMenu()  
    {  
        Show(mainMenu);
        Hide(settingsMenu);
    }

    void Show(CanvasGroup cg)
    {
        cg.alpha = 1;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    void Hide(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}