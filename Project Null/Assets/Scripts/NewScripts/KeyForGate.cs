using UnityEngine;

public class KeyForGate : MonoBehaviour, stage2_InteractableScript
{
    public void InteractScript()
    {
        Debug.Log("Gate key picked up.");

        if (Stage2ProgressManager.Instance != null)
        {
            Stage2ProgressManager.Instance.gateKeyPickedUp = true;
        }

        gameObject.SetActive(false);
    }
}