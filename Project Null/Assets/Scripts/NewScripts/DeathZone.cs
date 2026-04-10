using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            LifeManager.Instance.PlayerDied();
        }
    }
}
