using UnityEngine;

public class KeyForGate : MonoBehaviour, stage2_InteractableScript
{
    [SerializeField] private HUD hud;

    public void InteractScript()
    {
        Debug.Log("Gate key picked up.");
        hud.PickedUpGateKey();


        if (Stage2ProgressManager.Instance != null)
        {
            Stage2ProgressManager.Instance.gateKeyPickedUp = true;
        }


        gameObject.SetActive(false);
    }
}