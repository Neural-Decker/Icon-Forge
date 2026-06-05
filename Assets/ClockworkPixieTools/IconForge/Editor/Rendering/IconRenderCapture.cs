using UnityEngine;

public static class IconRenderCapture
{
    private const int PreviewLayer = 31;

    public static Texture2D GeneratePreview(GameObject sourceObject)
    {
        if (sourceObject == null)
        {
            Debug.LogWarning("Icon Forge: No source object provided.");
            return null;
        }

        int resolution = 256;

        GameObject previewRoot = new GameObject("Icon Forge Preview Root");
        GameObject previewObject = Object.Instantiate(sourceObject, previewRoot.transform);

        previewRoot.transform.position = Vector3.zero;
        previewObject.transform.localPosition = Vector3.zero;
        previewObject.transform.localRotation = Quaternion.identity;

        SetLayerRecursively(previewRoot, PreviewLayer);

        Camera camera = CreatePreviewCamera();
        Light light = CreatePreviewLight();

        SetLayerRecursively(camera.gameObject, PreviewLayer);
        SetLayerRecursively(light.gameObject, PreviewLayer);

        RenderTexture renderTexture = new RenderTexture(resolution, resolution, 24);
        camera.targetTexture = renderTexture;

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        GL.Clear(true, true, new Color(0.08f, 0.08f, 0.08f, 1f));

        camera.Render();

        Texture2D result = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        result.Apply();

        RenderTexture.active = previous;

        camera.targetTexture = null;
        renderTexture.Release();

        Object.DestroyImmediate(renderTexture);
        Object.DestroyImmediate(light.gameObject);
        Object.DestroyImmediate(camera.gameObject);
        Object.DestroyImmediate(previewRoot);

        return result;
    }

    private static Camera CreatePreviewCamera()
    {
        GameObject cameraObject = new GameObject("Icon Forge Preview Camera");
        Camera camera = cameraObject.AddComponent<Camera>();

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.08f, 0.08f, 1f);
        camera.cullingMask = 1 << PreviewLayer;
        camera.orthographic = true;
        camera.orthographicSize = 1.5f;
        camera.transform.position = new Vector3(2.5f, 2f, 2.5f);
        camera.transform.LookAt(Vector3.zero);

        return camera;
    }

    private static Light CreatePreviewLight()
    {
        GameObject lightObject = new GameObject("Icon Forge Preview Light");
        Light light = lightObject.AddComponent<Light>();

        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

        return light;
    }

    private static void SetLayerRecursively(GameObject target, int layer)
    {
        target.layer = layer;

        foreach (Transform child in target.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}