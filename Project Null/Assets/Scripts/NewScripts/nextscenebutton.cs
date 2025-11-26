using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorButtonSceneTransition : MonoBehaviour, InteractableScript, stage2_InteractableScript
{
    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load")]
    public string nextSceneName = "Stage2";

    [Header("Elevator Requirements")]
    [Tooltip("Reference to the elevator button - elevator must be open to transition")]
public MonoBehaviour elevatorButton;   // can hold ANY component

private bool ElevatorIsOpen
{
    get
    {
        if (elevatorButton == null) return false;

        // Try Stage 1 elevator
        if (elevatorButton is ElevatorButtonFixed fixedButton)
            return fixedButton.ElevatorOpened;

        // Try Stage 2 elevator
        if (elevatorButton is Stage2ElevatorButton stage2Button)
            return stage2Button.ElevatorOpened;

        return false;
    }
}



    [Header("Button Interaction")]
    [Tooltip("Key to press when looking at the button")]
    public KeyCode interactionKey = KeyCode.E;
    [Tooltip("Maximum distance to interact with button")]
    public float interactDistance = 3f;
    [Tooltip("Camera used for raycast (usually Main Camera)")]
    public Camera playerCamera;
    [Tooltip("Layer mask for interactable objects")]
    public LayerMask interactMask;

    [Header("UI Prompt (Optional)")]
    [Tooltip("UI element to show when looking at button")]
    public GameObject interactionPrompt;

    [Header("Visual Feedback (Optional)")]
    [Tooltip("Material to apply when button is interactable")]
    public Material interactableMaterial;
    [Tooltip("Material to apply when button is not interactable")]
    public Material disabledMaterial;
    private Renderer buttonRenderer;
    private Material originalMaterial;

    private bool playerLookingAtButton = false;

    // ✅ Elevator must exist and be open
private bool CanTransition => ElevatorIsOpen;


    private void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Store original material
        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer != null)
            originalMaterial = buttonRenderer.material;
    }

    // private void Update()
    // {
    //     CheckIfLookingAtButton();
    //
    //     // Try to interact when pressing key
    //     if (Input.GetKeyDown(interactionKey) && playerLookingAtButton)
    //     {
    //         TryTransition();
    //     }
    //
    //     // Update visual feedback
    //     UpdateButtonAppearance();
    // }

    private void CheckIfLookingAtButton()
    {
        if (playerCamera == null)
        {
            playerLookingAtButton = false;
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Raycast to see if looking at this button
        if (Physics.Raycast(ray, out hit, interactDistance, interactMask.value == 0 ? ~0 : interactMask))
        {
            // Check if looking directly at this button
            if (hit.collider.gameObject == gameObject)
            {
                playerLookingAtButton = true;

                // Show prompt only if elevator is ready
                if (interactionPrompt != null)
                    interactionPrompt.SetActive(CanTransition);

                return;
            }
        }

        // Not looking at button
        playerLookingAtButton = false;
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    public void InteractScript()
    {
        TryTransition();
    }

    private void TryTransition()
    {
        if (CanTransition)
        {
            Debug.Log("[ElevatorButton] Elevator is open. Loading next scene...");
            SaveAndLoadScene();
        }
        else
        {
            Debug.Log("[ElevatorButton] Elevator not ready yet — cannot load scene.");
            // Optional: Play error sound or show message
        }
    }

    private void SaveAndLoadScene()
    {
        // Try to save player state before transitioning
        SceneTransitionManager transitionManager = FindObjectOfType<SceneTransitionManager>();

        if (transitionManager != null)
        {
            // Use the transition manager to save and load
            transitionManager.ChangeScene(nextSceneName);
        }
        else
        {
            // Fallback: Try to save manually using GameManager
            if (GameManager.Instance != null)
            {
                var player = FindObjectOfType<Player>();
                var sanity = FindObjectOfType<Sanity>();
                var flashlight = FindObjectOfType<FlashlightToggle>();
                var inventory = FindObjectOfType<Inventory>();
                GameManager.Instance.SavePlayerState(player, sanity, flashlight, inventory);
            }
            else
            {
                Debug.LogWarning("[ElevatorButton] No GameManager found - state won't be saved!");
            }

            // Load the scene
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void UpdateButtonAppearance()
    {
        if (buttonRenderer == null) return;

        // Change material based on state
        if (interactableMaterial != null && disabledMaterial != null)
        {
            if (CanTransition)
                buttonRenderer.material = interactableMaterial;
            else
                buttonRenderer.material = disabledMaterial;
        }

        // Optional: Add emission/glow when looking at ready button
        if (playerLookingAtButton && CanTransition && buttonRenderer.material.HasProperty("_EmissionColor"))
        {
            buttonRenderer.material.SetColor("_EmissionColor", Color.green * 0.5f);
        }
    }

    // Visualize the interaction ray in editor
    private void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = CanTransition ? Color.green : Color.red;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance);
        }

        // Draw a small sphere at button location
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }

    private void OnDestroy()
    {
        // Restore original material
        if (buttonRenderer != null && originalMaterial != null)
            buttonRenderer.material = originalMaterial;
    }
}