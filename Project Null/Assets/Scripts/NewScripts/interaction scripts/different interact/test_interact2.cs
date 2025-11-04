using UnityEngine;

public class test_interact2 : MonoBehaviour, InteractableScript
{
    public void InteractScript()
    {
        Debug.Log("cylinder deleted");
        Destroy(gameObject);
    }
}
