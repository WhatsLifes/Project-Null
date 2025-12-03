using UnityEngine;

public class Stage2_5ProgressManager : MonoBehaviour
{
    public static Stage2_5ProgressManager Instance;

    [Header("Stage 2.5 Conditions")]
    public bool boyFlowerPickedUp = false;
    public bool girlFlowerPickedUp = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool AllFlowersCollected()
    {
        return boyFlowerPickedUp && girlFlowerPickedUp;
    }
}
