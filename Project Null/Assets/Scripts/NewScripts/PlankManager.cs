using System.Collections.Generic;
using UnityEngine;

public class PlankManager : MonoBehaviour
{
    [Header("All plank GameObjects in scene")]
    public GameObject[] allPlanks;

    [Range(0f, 1f)]
    [Tooltip("Fraction of planks that disappear on red light (e.g., 0.4 = 40%)")]
    public float disappearFraction = 0.4f;

    private List<GameObject> currentlyHidden = new List<GameObject>();

    public void OnRedLight()
    {
        RestoreAll();

        List<GameObject> shuffled = new List<GameObject>(allPlanks);

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        int hideCount = Mathf.RoundToInt(shuffled.Count * disappearFraction);

        for (int i = 0; i < hideCount; i++)
        {
            shuffled[i].SetActive(false);
            currentlyHidden.Add(shuffled[i]);
        }
    }

    public void OnGreenLight()
    {
        RestoreAll();
    }

    private void RestoreAll()
    {
        foreach (GameObject plank in currentlyHidden)
        {
            if (plank != null)
                plank.SetActive(true);
        }
        currentlyHidden.Clear();
    }
}