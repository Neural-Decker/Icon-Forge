using UnityEngine;

[CreateAssetMenu(
    fileName = "IconOutputSettings",
    menuName = "Clockwork Pixie/Icon Forge/Output Settings")]
public class IconOutputSettings : ScriptableObject
{
    public int resolution = 512;
    public bool transparentBackground = true;
    public string exportFolder = "Assets/GeneratedIcons";
}