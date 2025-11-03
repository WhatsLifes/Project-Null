using UnityEngine;

public class test_interact : MonoBehaviour, InteractableScript
{
    public void InteractScript()
    {
        Debug.Log("fuck you");
        Destroy(gameObject);
    }
}
