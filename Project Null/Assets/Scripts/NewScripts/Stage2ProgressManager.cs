using UnityEngine;

public class Stage2ProgressManager : MonoBehaviour
{
    public static Stage2ProgressManager Instance;

    [Header("Stage 2 Conditions")]
    public bool sonPhotoPickedUp = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetSonPhotoPickedUp(bool state)
    {
        sonPhotoPickedUp = state;
    }
}
