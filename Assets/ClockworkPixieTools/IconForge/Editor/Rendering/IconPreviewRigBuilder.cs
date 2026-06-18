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
        IconCameraPreset cameraPreset,
        IconLightingProfile lightingProfile)
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
        Light mainLight = CreateKeyLight(camera, lightingProfile);
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
        camera.aspect = 1f;
        camera.nearClipPlane = 0.0001f;
        camera.farClipPlane = 1000f;

        Vector3 cameraOffset = GetCameraOffset(cameraPreset);
        Vector3 cameraDirection = cameraOffset.normalized;
        camera.transform.position = bounds.center + cameraDirection * 50f;
        camera.transform.LookAt(bounds.center);

        fillPercent = Mathf.Clamp(fillPercent, 0.1f, 1f);
        camera.orthographicSize =
            CalculateCameraAlignedOrthographicSize(camera, bounds, fillPercent);

        return camera;
    }

    private static Light CreateKeyLight(
    Camera camera,
    IconLightingProfile profile)
    {
        GameObject lightObject = new GameObject("Icon Forge Key Light");
        Light light = lightObject.AddComponent<Light>();

        light.type = LightType.Directional;
        light.cullingMask = 1 << PreviewLayer;

        switch (profile)
        {
            case IconLightingProfile.Fantasy:
                light.intensity = 1.3f;
                light.color = new Color(1f, 0.92f, 0.78f);
                break;

            case IconLightingProfile.SciFi:
                light.intensity = 1.25f;
                light.color = new Color(0.75f, 0.9f, 1f);
                break;

            case IconLightingProfile.Stylized:
                light.intensity = 1.5f;
                light.color = Color.white;
                break;

            case IconLightingProfile.Minimal:
                light.intensity = 0.9f;
                light.color = Color.white;
                break;

            case IconLightingProfile.Neutral:
            default:
                light.intensity = 1.2f;
                light.color = Color.white;
                break;
        }

        Vector3 lightDirectionToObject =
            camera.transform.forward
            - camera.transform.right * 0.25f
            + camera.transform.up * 0.25f;

        light.transform.rotation =
            Quaternion.LookRotation(lightDirectionToObject.normalized);

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