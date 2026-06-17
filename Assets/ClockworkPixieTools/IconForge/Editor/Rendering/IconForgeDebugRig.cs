using UnityEngine;

public static class IconForgeDebugRig
{
    private const string DebugRigName = "Icon Forge Preview Rig";

    public static void CreateDebugRig(
        GameObject sourceObject,
        float fillPercent,
        Color backgroundColor,
        IconCameraPreset cameraPreset,
        IconLightingProfile lightingProfile)
    {
        DestroyDebugRig();

        IconPreviewRig rig = IconPreviewRigBuilder.Build(
            sourceObject,
            fillPercent,
            backgroundColor,
            cameraPreset,
            lightingProfile);

        if (rig == null)
        {
            return;
        }

        Debug.Log("Icon Forge: Debug rig created.");
    }

    public static void DestroyDebugRig()
    {
        GameObject existingRig = GameObject.Find(DebugRigName);

        if (existingRig != null)
        {
            Object.DestroyImmediate(existingRig);
            Debug.Log("Icon Forge: Debug rig destroyed.");
        }
    }
}