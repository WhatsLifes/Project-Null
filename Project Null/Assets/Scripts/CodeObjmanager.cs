using UnityEngine;

public class CodeManager : MonoBehaviour
{
    public static CodeManager Instance;

    [SerializeField] private HUD hud;
    private int codesFound = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void CodeFound()
    {
        codesFound++;

        switch (codesFound)
        {
            case 1: hud.ShowObjective6(); break; // 1/3
            case 2: hud.ShowObjective7(); break; // 2/3
            case 3: hud.ShowObjective8(); break; // all found, solve puzzle
        }
    }
}