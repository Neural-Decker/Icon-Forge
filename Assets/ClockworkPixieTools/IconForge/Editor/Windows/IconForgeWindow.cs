using UnityEditor;
using UnityEngine;

public class IconForgeWindow : EditorWindow
{
    [MenuItem("Tools/Clockwork Pixie/Icon Forge")]
    public static void ShowWindow()
    {
        GetWindow<IconForgeWindow>("Icon Forge");
    }

    private GameObject sourceObject;
    private IconForgeProfile profile;
    private IconOutputSettings outputSettings;
    private Texture2D logoTexture;
    private Texture2D footprintsTexture;
    private Texture2D previewTexture;
    private float iconFillPercent = 70f;

    private void OnGUI()
    {
        // TODO: low opasity footprints in the Background
        // Title
        // TODO: add Icon here, followed by Lable text
        GUILayout.Label("Clockwork Pixie Icon Forge", EditorStyles.boldLabel);

        EditorGUILayout.Space(10);

        // Source
        sourceObject = (GameObject)EditorGUILayout.ObjectField(
            "Source Object",
            sourceObject,
            typeof(GameObject),
            true);

        EditorGUILayout.Space(10);

        // Help Box
        EditorGUILayout.HelpBox(
            "Select a prefab or scene object to prepare for icon generation.",
            MessageType.Info);

        EditorGUILayout.Space(10);

        // Profile and Output Settings
        profile = (IconForgeProfile)EditorGUILayout.ObjectField(
            "Profile",
            profile,
            typeof(IconForgeProfile),
            false);

        outputSettings = (IconOutputSettings)EditorGUILayout.ObjectField(
            "Output Settings",
            outputSettings,
            typeof(IconOutputSettings),
            false);

        EditorGUILayout.Space(10);

        // Framing
        EditorGUILayout.LabelField("Framing", EditorStyles.boldLabel);

        iconFillPercent = EditorGUILayout.Slider("Icon Fill", iconFillPercent, 40f, 90f);
        iconFillPercent = Mathf.Round(iconFillPercent / 5f) * 5f;

        EditorGUILayout.Space(10);

        // Peview section
        DrawPreviewPanel();

        EditorGUILayout.Space(10);

        // Button to Generate preview
        GUI.enabled = sourceObject != null;

        if (GUILayout.Button("Generate Preview"))
        {
            Debug.Log($"Icon Forge preview requested for: {sourceObject.name}");
            previewTexture = IconRenderCapture.GeneratePreview(
                sourceObject,
                iconFillPercent / 100f);
        }

        GUI.enabled = true;
    }

    private void DrawPreviewPanel()
    {
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        DrawPreviewBox("Detail Preview", 256, 256);
        GUILayout.Space(16);
        DrawPreviewBox("64 x 64", 64, 64);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawPreviewBox(string label, int width, int height)
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(width));

        EditorGUILayout.LabelField(label, EditorStyles.centeredGreyMiniLabel);

        Rect outerRect = GUILayoutUtility.GetRect(
            width,
            height,
            GUILayout.Width(width),
            GUILayout.Height(height));

        // Border
        EditorGUI.DrawRect(outerRect, new Color(0.28f, 0.28f, 0.28f));

        // Inner preview background
        Rect innerRect = new Rect(
            outerRect.x + 1,
            outerRect.y + 1,
            outerRect.width - 2,
            outerRect.height - 2);

        EditorGUI.DrawRect(innerRect, new Color(0.12f, 0.12f, 0.12f));

        if (previewTexture != null)
        {
            GUI.DrawTexture(
                innerRect,
                previewTexture,
                ScaleMode.ScaleToFit);
        } else
        {
            GUI.Label(
                innerRect,
                "Preview",
                EditorStyles.centeredGreyMiniLabel);
        }

        EditorGUILayout.EndVertical();
    }
}