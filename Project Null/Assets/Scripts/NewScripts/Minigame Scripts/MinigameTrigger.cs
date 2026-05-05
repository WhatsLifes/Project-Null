using UnityEngine;

public class MinigameExit : MonoBehaviour
{
    public PlankManager plankManager;
    public MinigameManager minigameManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            minigameManager.gameObject.SetActive(true);
            plankManager.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            plankManager.OnGreenLight();
            minigameManager.gameStarted = false;

            minigameManager.gameObject.SetActive(false);
            plankManager.gameObject.SetActive(false);
        }
    }
}