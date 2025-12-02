using UnityEngine;
using System.Collections.Generic;

public class DollPuzzleManager : MonoBehaviour
{
    public static DollPuzzleManager Instance;

    public enum EyeType
    {
        X,
        Square,
        Circle,
        Triangle
    }

    [System.Serializable]
    public class ChairInfo
    {
        public ChairSlot chairSlot;
        public EyeType requiredLeftEye;
        public EyeType requiredRightEye;
    }

    public List<ChairInfo> chairs = new List<ChairInfo>();
    private List<DollBehavior> activeDolls = new List<DollBehavior>();

    [Header("Feedback")]
    public PuzzleFeedbackLight feedbackLight;

    [SerializeField] private HUD hud;

    public bool lastPuzzleResult { get; private set; } = false;

    void Awake()
    {
        Instance = this;
    }

    public bool AreAllChairsFilled()
    {
        foreach (var c in chairs)
        {
            if (c.chairSlot == null || c.chairSlot.currentDoll == null)
                return false;
        }
        return true;
    }

    public void CheckPuzzle()
    {
        if (!AreAllChairsFilled())
        {
            Debug.LogWarning("Cannot check puzzle — not all chairs have dolls!");
            return;
        }

        Debug.Log("Checking doll puzzle...");
        bool allCorrect = true;

        foreach (var c in chairs)
        {
            if (c.chairSlot == null || c.chairSlot.currentDoll == null)
                continue;

            DollBehavior doll = c.chairSlot.currentDoll.GetComponent<DollBehavior>();
            if (doll == null)
            {
                Debug.LogWarning($"Chair {c.chairSlot.name} has object without DollBehavior.");
                continue;
            }

            bool leftMatch = doll.leftEye == c.requiredLeftEye;
            bool rightMatch = doll.rightEye == c.requiredRightEye;

            if (leftMatch && rightMatch)
            {
                doll.OnCorrectPlacement();
                Debug.Log($"{doll.name} correctly placed on {c.chairSlot.name}");
            }
            else
            {
                allCorrect = false;

                bool hasCorrectEyes = MatchesAnyChair(doll);
                Debug.Log($"{doll.name} incorrectly placed (Correct eyes for another chair: {hasCorrectEyes})");

                doll.OnIncorrectPlacement(hasCorrectEyes);
                c.chairSlot.currentDoll = null;
            }
        }

        if (allCorrect)
        {
            Debug.Log("Puzzle solved correctly! Opening the door...");

            if (feedbackLight != null)
            {
                Debug.Log(">>> Flashing GREEN light for success!");
                feedbackLight.Flash(true);
            }

            hud.ShowObjective5();

            // ✅ NEW: mark puzzle as solved in global progress
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.puzzleCompleted = true;
                Debug.Log("✅ Puzzle progress marked true!");
            }

        }
        else
        {
            Debug.Log("Puzzle not correct. Door remains closed.");

            if (feedbackLight != null)
            {
                Debug.Log(">>> Flashing RED light for failure!");
                feedbackLight.Flash(false);
            }
        }

        lastPuzzleResult = allCorrect;
        Debug.Log("Puzzle check complete!");
    }

    private bool MatchesAnyChair(DollBehavior doll)
    {
        foreach (var c in chairs)
        {
            if (doll.leftEye == c.requiredLeftEye && doll.rightEye == c.requiredRightEye)
                return true;
        }
        return false;
    }

    public void RegisterDoll(DollBehavior doll)
    {
        if (!activeDolls.Contains(doll))
            activeDolls.Add(doll);
    }

    public void UnregisterDoll(DollBehavior doll)
    {
        if (activeDolls.Contains(doll))
            activeDolls.Remove(doll);
    }

    public void OnDollPlaced(ChairSlot slot) { }
    public void OnDollRemoved(ChairSlot slot) { }
}
