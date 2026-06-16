using UnityEngine;

public static class IconPreviewRigBuilder
{
    private const int PreviewLayer = 31;
    private const string RigName = "Icon Forge Preview Rig";
    private static readonly Vector3 RigPosition = new Vector3(1000f, 0f, 1000f);
    private const float FrameSafePaddingMultiplier = 1.15f;

    public static IconPreviewRig Build(
        GameObject sourceObject,
        float fillPercent,
        Color backgroundColor,
        IconCameraPreset cameraPreset)
    {
        if (sourceObject == null)
        {
            Debug.LogWarning("Icon Forge: No source object provided.");
            return null;
        }

        GameObject root = new GameObject(RigName);
        root.transform.position = RigPosition;

        GameObject previewObject = Object.Instantiate(sourceObject, root.transform);
        previewObject.name = "Icon Forge Preview Object";
        previewObject.transform.localPosition = Vector3.zero;
        previewObject.transform.localRotation = Quaternion.identity;

        SetLayerRecursively(root, PreviewLayer);

        Bounds bounds = ObjectBoundsAnalyzer.CalculateRendererBounds(previewObject);

        Camera camera = CreateCamera(bounds, fillPercent, backgroundColor, cameraPreset);
        camera.transform.SetParent(root.transform);

        //Lighting
        Light mainLight = CreateLight(camera);
        mainLight.transform.SetParent(root.transform);

        SetLayerRecursively(root, PreviewLayer);

        return new IconPreviewRig
        {
            Root = root,
            PreviewObject = previewObject,
            Camera = camera,
            MainLight = mainLight,
            ObjectBounds = bounds
        };
    }

    private static Camera CreateCamera(
        Bounds bounds,
        float fillPercent,
        Color backgroundColor,
        IconCameraPreset cameraPreset)
    {
        GameObject cameraObject = new GameObject("Icon Forge Preview Camera");
        Camera camera = cameraObject.AddComponent<Camera>();

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = backgroundColor;
        camera.cullingMask = 1 << PreviewLayer;
        camera.orthographic = true;
        camera.nearClipPlane = 0.0001f;
        camera.farClipPlane = 1000f;

        Vector3 cameraOffset = GetCameraOffset(cameraPreset);
        Vector3 cameraDirection = cameraOffset.normalized;
        camera.transform.position = bounds.center + cameraDirection * 20f;
        camera.transform.LookAt(bounds.center);

        fillPercent = Mathf.Clamp(fillPercent, 0.1f, 1f);
        camera.orthographicSize =
            CalculateCameraAlignedOrthographicSize(camera, bounds, fillPercent);

        return camera;
    }

    private static Light CreateLight(Camera camera)
    {
        GameObject lightObject = new GameObject("Icon Forge Preview Light");
        Light light = lightObject.AddComponent<Light>();

        light.type = LightType.Directional;
        light.intensity = 1.2f;

        Vector3 cameraForward = camera.transform.forward;
        Vector3 cameraRight = camera.transform.right;
        Vector3 cameraUp = camera.transform.up;

        Vector3 lightDirection =
            cameraForward
            - cameraRight * 0.35f
            + cameraUp * 0.45f;

        light.transform.rotation = Quaternion.LookRotation(lightDirection.normalized);
        light.transform.position = camera.transform.position;

        return light;
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

        return (requiredSize / fillPercent) * FrameSafePaddingMultiplier;
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

    private static void SetLayerRecursively(GameObject target, int layer)
    {
        target.layer = layer;

        foreach (Transform child in target.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private static Vector3 GetCameraOffset(IconCameraPreset preset)
    {
        switch (preset)
        {
            case IconCameraPreset.Front:
                return new Vector3(0f, 0f, 5f);

            case IconCameraPreset.Side:
                return new Vector3(5f, 0f, 0f);

            case IconCameraPreset.Top:
                return new Vector3(0f, 5f, 0.001f);

            case IconCameraPreset.Isometric:
            default:
                return new Vector3(2.5f, 2f, 2.5f);
        }
    }
}