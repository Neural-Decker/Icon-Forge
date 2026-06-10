using UnityEditor;
using UnityEngine;
using System.IO;

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
    private bool showDebugSection = false;
    private string exportFileName = "IconForge_Icon.png";

    private void OnGUI()
    {
        // TODO: low opasity footprints in the Background
        // Title
        // TODO: add Icon here, followed by Lable text
        GUILayout.Label("Clockwork Pixie Icon Forge", EditorStyles.boldLabel);

        EditorGUILayout.Space(10);

        // Source
        EditorGUI.BeginChangeCheck();
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

        exportFileName = EditorGUILayout.TextField(
            "Export File Name",
            exportFileName);

        EditorGUILayout.Space(10);

        // Framing
        EditorGUILayout.LabelField("Framing", EditorStyles.boldLabel);

        iconFillPercent = EditorGUILayout.Slider("Icon Fill", iconFillPercent, 40f, 90f);
        iconFillPercent = Mathf.Round(iconFillPercent / 5f) * 5f;

        if (EditorGUI.EndChangeCheck())
        {
            GeneratePreview();
        }

        //Debuging
        EditorGUILayout.Space(8);

        showDebugSection = EditorGUILayout.Foldout(
            showDebugSection,
            "Debug",
            true);

        if (showDebugSection)
        {
            EditorGUI.indentLevel++;

            GUI.enabled = sourceObject != null;

            if (GUILayout.Button("Create Debug Rig"))
            {
                IconForgeDebugRig.CreateDebugRig(
                    sourceObject,
                    iconFillPercent / 100f);
            }

            GUI.enabled = true;

            if (GUILayout.Button("Destroy Debug Rig"))
            {
                IconForgeDebugRig.DestroyDebugRig();
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // Peview section
        DrawPreviewPanel();

        EditorGUILayout.Space(10);

        // Button to Generate preview
        GUI.enabled = sourceObject != null;

        if (GUILayout.Button("Generate Preview"))
        {
            GeneratePreview();
        }
        if (GUILayout.Button("Export PNG"))
        {
            ExportPreviewPng();
        }

        GUI.enabled = true;
    }

    private void GeneratePreview()
    {
        if (sourceObject == null)
        {
            previewTexture = null;
            return;
        }

        previewTexture = IconRenderCapture.GeneratePreview(
            sourceObject,
            iconFillPercent / 100f);

        Repaint();
    }

    private void ExportPreviewPng()
    {
        if (previewTexture == null)
        {
            Debug.LogWarning("Icon Forge: No preview texture available to export.");
            return;
        }

        string exportFolder = "Assets/GeneratedIcons";

        if (outputSettings != null && !string.IsNullOrWhiteSpace(outputSettings.exportFolder))
        {
            exportFolder = outputSettings.exportFolder;
        }

        if (!Directory.Exists(exportFolder))
        {
            Directory.CreateDirectory(exportFolder);
        }

        string safeFileName = exportFileName;

        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            safeFileName = "IconForge_Icon.png";
        }

        if (!safeFileName.EndsWith(".png"))
        {
            safeFileName += ".png";
        }

        string fullPath = Path.Combine(exportFolder, safeFileName);

        byte[] pngData = previewTexture.EncodeToPNG();
        File.WriteAllBytes(fullPath, pngData);

        AssetDatabase.Refresh();

        Debug.Log($"Icon Forge: Exported PNG to {fullPath}");
    }

    private void DrawCheckerboard(Rect rect)
    {
        int tileSize = 16;

        Color colorA = new Color(0.35f, 0.35f, 0.35f);
        Color colorB = new Color(0.25f, 0.25f, 0.25f);

        for (int y = 0; y < rect.height; y += tileSize)
        {
            for (int x = 0; x < rect.width; x += tileSize)
            {
                Rect tile = new Rect(
                    rect.x + x,
                    rect.y + y,
                    tileSize,
                    tileSize);

                bool even = ((x / tileSize) + (y / tileSize)) % 2 == 0;

                EditorGUI.DrawRect(tile, even ? colorA : colorB);
            }
        }
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

        DrawCheckerboard(innerRect);

        if (previewTexture != null)
        {
            GUI.DrawTexture(
                innerRect,
                previewTexture,
                ScaleMode.ScaleToFit,
                true);
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