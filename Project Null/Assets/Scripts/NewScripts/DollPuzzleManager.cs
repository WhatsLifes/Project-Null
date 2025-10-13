using UnityEngine;
using System.Collections.Generic;

public class DollPuzzleManager : MonoBehaviour
{
    public static DollPuzzleManager Instance;

    [System.Serializable]
    public class ChairInfo
    {
        public ChairSlot chairSlot; // reference to your ChairSlotLogic
        public string requiredLeftEye;
        public string requiredRightEye;
    }

    public List<ChairInfo> chairs = new List<ChairInfo>();
    private List<DollBehavior> activeDolls = new List<DollBehavior>();

    [Header("Door to open when puzzle is solved")]
    public Door door; // Assign your door here in inspector

    // ✅ Added field to store last puzzle result
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

            // Normalize for safer string comparison
            string dollLeft = doll.leftEye.Trim().ToLower();
            string dollRight = doll.rightEye.Trim().ToLower();
            string requiredLeft = c.requiredLeftEye.Trim().ToLower();
            string requiredRight = c.requiredRightEye.Trim().ToLower();

            bool leftMatch = dollLeft == requiredLeft;
            bool rightMatch = dollRight == requiredRight;

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
            if (door != null)
                door.OpenDoor();
        }
        else
        {
            Debug.Log("Puzzle not correct. Door remains closed.");
        }

        // ✅ Add this line to store the result
        lastPuzzleResult = allCorrect;

        Debug.Log("Puzzle check complete!");
    }

    private bool MatchesAnyChair(DollBehavior doll)
    {
        string dollLeft = doll.leftEye.Trim().ToLower();
        string dollRight = doll.rightEye.Trim().ToLower();

        foreach (var c in chairs)
        {
            string reqLeft = c.requiredLeftEye.Trim().ToLower();
            string reqRight = c.requiredRightEye.Trim().ToLower();
            if (dollLeft == reqLeft && dollRight == reqRight)
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
