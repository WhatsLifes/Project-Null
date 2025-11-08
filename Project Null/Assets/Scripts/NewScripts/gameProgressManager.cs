using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance;

    [Header("Global Conditions")]
    public bool buttonPressed = false;
    public bool puzzleCompleted = false;
    public bool dadPiecePickedUp = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Optional helper
    public bool CanOpenFinalDoor()
    {
        return buttonPressed && puzzleCompleted;
    }
}
