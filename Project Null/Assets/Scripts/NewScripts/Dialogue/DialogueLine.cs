using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public bool isDoctor;
    public AudioClip voiceClip;
    [TextArea] public string playerText;
    public float delayBeforeNext = 1f;
}