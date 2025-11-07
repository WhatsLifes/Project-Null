using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextSceneWithKey : MonoBehaviour
{
    [Header("Scene Settings")]
    public string nextSceneName = "Stage2";
    public KeyCode loadKey = KeyCode.E;

    [Header("Interaction Settings")]
    public float interactDistance = 3f;
    public Camera playerCamera;
    public LayerMask interactMask;

    [Header("Elevator Dependencies")]
    public ElevatorButtonFixed elevatorButton; // Reference to outside button

    // ✅ Elevator must exist and be open
    private bool CanLoad => elevatorButton != null && elevatorButton.ElevatorOpened;

    void Update()
    {
        if (Input.GetKeyDown(loadKey))
            TryInteract();
    }

    private void TryInteract()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("[LoadNextSceneWithKey] Player Camera not assigned!");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Only hit interactable layers, or all if none set
        if (Physics.Raycast(ray, out hit, interactDistance, interactMask.value == 0 ? ~0 : interactMask))
        {
            // Check if looking directly at this button
            if (hit.collider.gameObject == gameObject)
            {
                if (CanLoad)
                {
                    Debug.Log("[LoadNextSceneWithKey] Elevator is open. Loading next scene...");
                    SceneManager.LoadScene(nextSceneName);
                }
                else
                {
                    Debug.Log("[LoadNextSceneWithKey] Elevator not ready yet — cannot load scene.");
                }
            }
        }
    }

    // Optional: visualize the interaction ray
    void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance);
        }
    }
}
