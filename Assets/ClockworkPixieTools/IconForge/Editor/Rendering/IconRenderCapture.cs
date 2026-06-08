using UnityEngine;

public static class IconRenderCapture
{
    public static Texture2D GeneratePreview(GameObject sourceObject, float fillPercent)
    {
        IconPreviewRig rig = IconPreviewRigBuilder.Build(sourceObject, fillPercent);

        if (rig == null)
        {
            return null;
        }

        int resolution = 256;

        RenderTexture renderTexture = new RenderTexture(resolution, resolution, 24);
        rig.Camera.targetTexture = renderTexture;

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));

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