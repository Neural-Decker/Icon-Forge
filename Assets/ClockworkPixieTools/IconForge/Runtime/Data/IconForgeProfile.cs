using UnityEngine;

[CreateAssetMenu(
    fileName = "IconForgeProfile",
    menuName = "Clockwork Pixie/Icon Forge Profile")]
public class IconForgeProfile : ScriptableObject
{
    [Header("Presets")]
    public IconItemTypePreset itemType;

    public IconCameraPreset cameraPreset;

    public IconLightingProfile lightingProfile;

    [Header("Rendering")]
    public float fillPercent = 80f;

    public int resolution = 512;

    public bool transparentBackground = true;

    public Color backgroundColor = Color.clear;
}