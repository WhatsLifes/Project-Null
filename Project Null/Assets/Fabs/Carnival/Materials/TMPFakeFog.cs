using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class TMPFakeFog : MonoBehaviour
{
    public float fogStart = 5f;
    public float fogEnd = 20f;

    [Header("Color Settings")]
    public Color baseColor = Color.white;
    public Color fogColor = Color.gray;

    TextMeshPro tmp;
    Camera cam;

    void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
        cam = Camera.main;
    }

    void Update()
    {
        if (cam == null) return;

        float dist = Vector3.Distance(cam.transform.position, transform.position);

        float t = Mathf.InverseLerp(fogStart, fogEnd, dist);

        // Blend color toward fog color
        float alpha = Mathf.Lerp(1f, 0f, t);
        Color c = tmp.color;
        c.a = alpha;
        tmp.color = c;

    }
}