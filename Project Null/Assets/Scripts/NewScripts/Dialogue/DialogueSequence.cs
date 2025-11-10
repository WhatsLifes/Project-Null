using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueSequence", menuName = "Dialogue/Sequence")]
public class DialogueSequence : ScriptableObject
{
    public string sequenceName;
    public DialogueLine[] lines;
}