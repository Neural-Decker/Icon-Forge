using UnityEngine;

public static class IconRenderCapture
{
    private const int PreviewLayer = 31;

    public static Texture2D GeneratePreview(GameObject sourceObject, float fillPercent)
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

        Bounds objectBounds = ObjectBoundsAnalyzer.CalculateRendererBounds(previewObject);
        Debug.Log($"Icon Forge Bounds: Center={objectBounds.center}, Size={objectBounds.size}");

        Camera camera = CreatePreviewCamera(objectBounds, fillPercent);
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

    private static Camera CreatePreviewCamera(Bounds objectBounds, float fillPercent)
    {
        GameObject cameraObject = new GameObject("Icon Forge Preview Camera");
        Camera camera = cameraObject.AddComponent<Camera>();

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.08f, 0.08f, 1f);
        camera.cullingMask = 1 << PreviewLayer;
        camera.orthographic = true;

        camera.transform.position = new Vector3(2.5f, 2f, 2.5f);
        camera.transform.LookAt(objectBounds.center);

        fillPercent = Mathf.Clamp(fillPercent, 0.1f, 1f);

        float requiredOrthographicSize = CalculateCameraAlignedOrthographicSize(
            camera,
            objectBounds,
            fillPercent);

        camera.orthographicSize = requiredOrthographicSize;

        return camera;
    }

    private static float CalculateCameraAlignedOrthographicSize(Camera camera, Bounds bounds, float fillPercent)
    {
        Vector3[] corners = GetBoundsCorners(bounds);

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        Matrix4x4 worldToCamera = camera.worldToCameraMatrix;

        foreach (Vector3 corner in corners)
        {
            Vector3 cameraSpaceCorner = worldToCamera.MultiplyPoint(corner);

            minX = Mathf.Min(minX, cameraSpaceCorner.x);
            maxX = Mathf.Max(maxX, cameraSpaceCorner.x);
            minY = Mathf.Min(minY, cameraSpaceCorner.y);
            maxY = Mathf.Max(maxY, cameraSpaceCorner.y);
        }

        float cameraSpaceWidth = maxX - minX;
        float cameraSpaceHeight = maxY - minY;

        float requiredSizeByHeight = cameraSpaceHeight / 2f;
        float requiredSizeByWidth = cameraSpaceWidth / (2f * camera.aspect);

        float requiredSize = Mathf.Max(requiredSizeByHeight, requiredSizeByWidth);

        return requiredSize / fillPercent;
    }

    private static Vector3[] GetBoundsCorners(Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        return new Vector3[]
        {
        center + new Vector3(-extents.x, -extents.y, -extents.z),
        center + new Vector3(-extents.x, -extents.y,  extents.z),
        center + new Vector3(-extents.x,  extents.y, -extents.z),
        center + new Vector3(-extents.x,  extents.y,  extents.z),

        center + new Vector3( extents.x, -extents.y, -extents.z),
        center + new Vector3( extents.x, -extents.y,  extents.z),
        center + new Vector3( extents.x,  extents.y, -extents.z),
        center + new Vector3( extents.x,  extents.y,  extents.z),
        };
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