using UnityEngine;

public static class IconPreviewRigBuilder
{
    private const int PreviewLayer = 31;
    private const string RigName = "Icon Forge Preview Rig";
    private static readonly Vector3 RigPosition = new Vector3(1000f, 0f, 1000f);

    public static IconPreviewRig Build(GameObject sourceObject, float fillPercent)
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

        Camera camera = CreateCamera(bounds, fillPercent);
        camera.transform.SetParent(root.transform);

        Light mainLight = CreateLight();
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

    private static Camera CreateCamera(Bounds bounds, float fillPercent)
    {
        GameObject cameraObject = new GameObject("Icon Forge Preview Camera");
        Camera camera = cameraObject.AddComponent<Camera>();

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        camera.cullingMask = 1 << PreviewLayer;
        camera.orthographic = true;

        camera.transform.position = RigPosition + new Vector3(2.5f, 2f, 2.5f);
        camera.transform.LookAt(bounds.center);

        fillPercent = Mathf.Clamp(fillPercent, 0.1f, 1f);
        camera.orthographicSize =
            CalculateCameraAlignedOrthographicSize(camera, bounds, fillPercent);

        return camera;
    }

    private static Light CreateLight()
    {
        GameObject lightObject = new GameObject("Icon Forge Preview Light");
        Light light = lightObject.AddComponent<Light>();

        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.transform.position = RigPosition + new Vector3(0f, 3f, 0f);
        light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

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

    private static void SetLayerRecursively(GameObject target, int layer)
    {
        target.layer = layer;

        foreach (Transform child in target.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}