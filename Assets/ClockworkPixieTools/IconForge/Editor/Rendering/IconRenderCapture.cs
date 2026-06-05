using UnityEngine;

public static class IconRenderCapture
{
    public static Texture2D GeneratePreview(GameObject sourceObject)
    {
        Debug.Log($"Generating preview for: {sourceObject.name}");

        return Texture2D.grayTexture;
    }
}