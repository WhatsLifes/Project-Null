using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBlurEffect : MonoBehaviour
{
    public Material blurMaterial;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (blurMaterial != null)
        {
            Graphics.Blit(src, dest, blurMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}