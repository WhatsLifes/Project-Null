using UnityEngine;

public class SettingsMenuManager : MonoBehaviour
{
    public GameObject controlsPanel;
    public GameObject visualsPanel;
    public GameObject audioPanel;
    public GameObject creditsPanel;

    public void ShowControls()
    {
        HideAllPanels();
        controlsPanel.SetActive(true);
    }

    public void ShowVisuals()
    {
        HideAllPanels();
        visualsPanel.SetActive(true);
    }

    public void ShowAudio()
    {
        HideAllPanels();
        audioPanel.SetActive(true);
    }

    public void ShowCredits()
    {
        HideAllPanels();
        creditsPanel.SetActive(true);
    }

    public void ReturnToSettings()
    {
        HideAllPanels();
    }

    private void HideAllPanels()
    {
        controlsPanel.SetActive(false);
        visualsPanel.SetActive(false);
        audioPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }
}

