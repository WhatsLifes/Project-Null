using UnityEngine;

public class InteractableDoll : MonoBehaviour, InteractableScript
{
    private DollBehavior dollBehavior;

    void Awake()
    {
        dollBehavior = GetComponent<DollBehavior>();
    }

    public void InteractScript()
    {
        if (dollBehavior != null && dollBehavior.type == DollBehavior.DollType.Hostile)
        {
            Debug.Log("Interacting with hostile doll - triggering activation");
            dollBehavior.OnPickedUp();
            return;
        }

        if (PlayerHold.Instance != null && !PlayerHold.Instance.IsHoldingObject())
        {
            PlayerHold.Instance.Pickup(this.gameObject);
        }
    }
}