using System.Collections.Generic;
using UnityEngine;

public static class IconRenderCapture
{
    public static Texture2D GeneratePreview(
        GameObject sourceObject,
        float fillPercent,
        int resolution,
        Color backgroundColor,
        IconCameraPreset cameraPreset,
        IconLightingProfile lightingProfile,
        Vector3 objectRotationOffset,
        Vector2 objectCompositionOffset)
    {
        IconPreviewRig rig = IconPreviewRigBuilder.Build(
            sourceObject,
            fillPercent,
            backgroundColor,
            cameraPreset,
            lightingProfile,
            objectRotationOffset,
            objectCompositionOffset);

        if (rig == null)
        {
            return null;
        }

        RenderTexture renderTexture = new RenderTexture(resolution, resolution, 24);
        rig.Camera.targetTexture = renderTexture;

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        GL.Clear(true, true, backgroundColor);

        Light[] sceneLights = Object.FindObjectsByType<Light>();

        List<Light> disabledSceneLights = new List<Light>();

        foreach (Light sceneLight in sceneLights)
        {
            if (sceneLight == null)
            {
                continue;
            }

            if (sceneLight.transform.IsChildOf(rig.Root.transform))
            {
                continue;
            }

            if (sceneLight.enabled)
            {
                sceneLight.enabled = false;
                disabledSceneLights.Add(sceneLight);
            }
        }

        rig.Camera.Render();

        foreach (Light disabledLight in disabledSceneLights)
        {
            if (disabledLight != null)
            {
                disabledLight.enabled = true;
            }
        }

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