using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MonitorController : MonoBehaviour
{
    [Tooltip("Index 0 = 3 lives, 1 = 2 lives, 2 = 1 life, 3 = 0 lives / dead")]
    public Material[] lifeDisplayMaterials;
    private DecalProjector decalProjector;

    private void Awake()
    {
        decalProjector = GetComponent<DecalProjector>();
    }

    private void OnEnable()
    {
        LifeManager.OnLivesChanged += UpdateDisplay;
    }

    private void OnDisable()
    {
        LifeManager.OnLivesChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(int livesRemaining)
    {
        
    
        int index = Mathf.Clamp(3 - livesRemaining, 0, lifeDisplayMaterials.Length - 1);

        if (decalProjector != null && lifeDisplayMaterials[index] != null)
        {
            decalProjector.material = lifeDisplayMaterials[index];
        }
    }
}
