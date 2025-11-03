using UnityEngine;

[CreateAssetMenu(fileName = "InteractableItemData", menuName = "Scriptable Objects/InteractableItemData")]
public class InteractableItemData : ScriptableObject
{
    public string itemName;
    public string interactionPrompt;
}
