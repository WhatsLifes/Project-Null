using UnityEngine;

public class Stage2ProgressManager : MonoBehaviour
{
    public static Stage2ProgressManager Instance;

    [Header("Stage 2 Conditions")]
    public bool sonPhotoPickedUp = false;
    public bool gateKeyPickedUp = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}
