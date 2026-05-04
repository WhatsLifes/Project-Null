using UnityEngine;

public class TriggerObjective5 : MonoBehaviour
{
    [SerializeField] private HUD hud;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        hud.ShowObjective5();
    }
}