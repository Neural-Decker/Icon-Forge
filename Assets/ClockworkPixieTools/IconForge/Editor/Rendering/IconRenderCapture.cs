using UnityEngine;

public static class IconRenderCapture
{
    public static Texture2D GeneratePreview(
    GameObject sourceObject,
    float fillPercent,
    int resolution,
    Color backgroundColor,
    IconCameraPreset cameraPreset)
    {
        IconPreviewRig rig = IconPreviewRigBuilder.Build(
            sourceObject,
            fillPercent,
            backgroundColor,
            cameraPreset);

        if (rig == null)
        {
            return null;
        }

        RenderTexture renderTexture = new RenderTexture(resolution, resolution, 24);
        rig.Camera.targetTexture = renderTexture;

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        GL.Clear(true, true, backgroundColor);

        rig.Camera.Render();

        Texture2D result = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        result.Apply();

        RenderTexture.active = previous;

        rig.Camera.targetTexture = null;
        renderTexture.Release();
        Object.DestroyImmediate(renderTexture);

        rig.Destroy();

        return result;
    }
}