using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public static DeathZone Instance;
    private bool triggered = false;
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !triggered)
        {
            LifeManager.Instance.PlayerDied();
            triggered = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !triggered)
        {
            LifeManager.Instance.PlayerDied();
            triggered = true;
        }
    }

    public void resetTrigger()
    {
        triggered = false;
    }
}
