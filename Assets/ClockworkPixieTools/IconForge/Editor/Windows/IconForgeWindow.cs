using System.Collections.Generic;
using System.IO;
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
    private bool showDebugSection = false;
    private string exportFileName = "IconForge_Icon.png";
    private readonly int[] resolutionValues =
    {
        64,
        128,
        256,
        512
    };
    private readonly string[] resolutionLabels =
    {
        "64 × 64",
        "128 × 128",
        "256 × 256",
        "512 × 512"
    };
    private bool useTransparentBackground = true;
    private Color backgroundColor = new Color(0.08f, 0.08f, 0.08f, 1f);
    private int selectedResolutionIndex = 2;
    private readonly List<GameObject> batchObjects = new List<GameObject>();
    private bool showBatchList = true;
    private Vector2 batchListScrollPosition;

    private void OnGUI()
    {
        // TODO: low opasity footprints in the Background
        // Menu section - Title
        // TODO: add Icon here, followed by Lable text
        GUILayout.Label("Clockwork Pixie Icon Forge", EditorStyles.boldLabel);

        EditorGUILayout.Space(10);

        // ====================
        // [UI] Source
        // ====================
        EditorGUI.BeginChangeCheck();
        sourceObject = (GameObject)EditorGUILayout.ObjectField("Source Object", sourceObject, typeof(GameObject), true);

        EditorGUILayout.Space(10);

        // ====================
        // [UI] Help Box
        // ====================
        EditorGUILayout.HelpBox("Select a prefab or scene object to prepare for icon generation.", MessageType.Info);

        EditorGUILayout.Space(10);

        // ====================
        // [UI] Profile
        // ====================
        profile = (IconForgeProfile)EditorGUILayout.ObjectField("Profile", profile, typeof(IconForgeProfile), false);
        
        EditorGUILayout.Space(10);

        // ====================
        // [UI] Batch processing
        // ====================
        EditorGUILayout.LabelField("Batch Processing", EditorStyles.boldLabel);
        
        // Enable
        GUI.enabled = sourceObject != null;

        // Button - Add single object to batchlist (Only active when object selected in sorce object field)
        if (GUILayout.Button("Add Source Object to Batch"))
        {
            if (!batchObjects.Contains(sourceObject))
            {
                batchObjects.Add(sourceObject);
            }
        }

        GUI.enabled = true;

        // Button - Add multiple objects from hierarchy 
        if (GUILayout.Button("Add Selected Hierarchy Objects to Batch"))
        {
            AddSelectedHierarchyObjectsToBatch();
        }

        // Button - Clear batch list
        if (GUILayout.Button("Clear Batch List"))
        {
            batchObjects.Clear();
        }

        // Batch counter
        EditorGUILayout.LabelField($"Batch Count: {batchObjects.Count}");

        // Display list of objects in batch list
        showBatchList = EditorGUILayout.Foldout(showBatchList, "Batch List", true);

        if (showBatchList)
        {
            if (batchObjects.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Batch list is empty. Add source objects or selected hierarchy objects.",
                    MessageType.Info);
            } else
            {
                float rowHeight = EditorGUIUtility.singleLineHeight + 4f;
                float maxVisibleRows = 10f;
                float scrollHeight = Mathf.Min(batchObjects.Count, maxVisibleRows) * rowHeight;

                batchListScrollPosition = EditorGUILayout.BeginScrollView(
                    batchListScrollPosition,
                    GUILayout.Height(scrollHeight));

                for (int i = 0; i < batchObjects.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    batchObjects[i] = (GameObject)EditorGUILayout.ObjectField(
                        batchObjects[i],
                        typeof(GameObject),
                        false);

                    if (GUILayout.Button("X", GUILayout.Width(24)))
                    {
                        batchObjects.RemoveAt(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }
        }

        EditorGUILayout.Space(10);

        // ======================
        // [UI] Output Settings
        // ======================
        outputSettings = (IconOutputSettings)EditorGUILayout.ObjectField("Output Settings", outputSettings, typeof(IconOutputSettings), false);
        EditorGUILayout.Space(10);

        // ======================
        // [UI] Background Transperent and Color
        // ======================
        useTransparentBackground = EditorGUILayout.Toggle("Transparent Background", useTransparentBackground);

        if (!useTransparentBackground)
        {
            backgroundColor = EditorGUILayout.ColorField("Background Color", backgroundColor);
        }
        
        // ======================
        // [UI] Resolution
        // ======================
        selectedResolutionIndex = EditorGUILayout.Popup("Export Resolution", selectedResolutionIndex, resolutionLabels);
        
        // ======================
        // [UI] File naming
        // ======================
        exportFileName = EditorGUILayout.TextField("Export File Name", exportFileName);

        EditorGUILayout.Space(10);

        // ======================
        // [UI] Framing
        // ======================
        EditorGUILayout.LabelField("Framing", EditorStyles.boldLabel);

        iconFillPercent = EditorGUILayout.Slider("Icon Fill", iconFillPercent, 40f, 90f);
        iconFillPercent = Mathf.Round(iconFillPercent / 5f) * 5f;

        if (EditorGUI.EndChangeCheck())
        {
            GeneratePreview();
        }
        EditorGUILayout.Space(10);

        // ======================
        // [UI] Debuging
        // ======================
        showDebugSection = EditorGUILayout.Foldout(showDebugSection, "Debug", true);

        if (showDebugSection)
        {
            EditorGUI.indentLevel++;

            GUI.enabled = sourceObject != null;

            // Button - create rig
            if (GUILayout.Button("Create Debug Rig"))
            {
                IconForgeDebugRig.CreateDebugRig(
                    sourceObject,
                    iconFillPercent / 100f,
                    backgroundColor);
            }

            GUI.enabled = true;

            // Button - Destroy rig
            if (GUILayout.Button("Destroy Debug Rig"))
            {
                IconForgeDebugRig.DestroyDebugRig();
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // ========================
        // [UI] Peview section
        // ========================
        DrawPreviewPanel();

        EditorGUILayout.Space(10);

        // ========================
        // [UI] Buttons
        // ========================
        GUI.enabled = sourceObject != null;

        if (GUILayout.Button("Generate Preview"))
        {
            GeneratePreview();
        }
        if (GUILayout.Button("Export PNG"))
        {
            ExportPreviewPng();
        }
        GUI.enabled = batchObjects.Count > 0;

        if (GUILayout.Button("Export Batch PNGs"))
        {
            ExportBatchPngs();
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

        Color renderBackgroundColor = useTransparentBackground ? new Color(0f, 0f, 0f, 0f) : backgroundColor;

        previewTexture = IconRenderCapture.GeneratePreview(sourceObject, iconFillPercent / 100f, 256, renderBackgroundColor);

        Repaint();
    }

    private void ExportPreviewPng()
    {
        if (sourceObject == null)
        {
            Debug.LogWarning("Icon Forge: No source object selected for export.");
            return;
        }

        int exportResolution = resolutionValues[selectedResolutionIndex];

        Color renderBackgroundColor = useTransparentBackground ? new Color(0f, 0f, 0f, 0f) : backgroundColor;

        Texture2D exportTexture = IconRenderCapture.GeneratePreview(sourceObject, iconFillPercent / 100f, exportResolution, renderBackgroundColor);

        if (exportTexture == null)
        {
            Debug.LogWarning("Icon Forge: Could not generate export texture.");
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

        byte[] pngData = exportTexture.EncodeToPNG();
        File.WriteAllBytes(fullPath, pngData);

        AssetDatabase.Refresh();

        Debug.Log($"Icon Forge: Exported {exportResolution}x{exportResolution} PNG to {fullPath}");
    }

    private void ExportBatchPngs()
    {
        if (batchObjects.Count == 0)
        {
            Debug.LogWarning("Icon Forge: Batch list is empty.");
            return;
        }

        foreach (GameObject batchObject in batchObjects)
        {
            if (batchObject == null)
            {
                continue;
            }

            sourceObject = batchObject;
            exportFileName = $"{batchObject.name}.png";

            ExportPreviewPng();
        }

        AssetDatabase.Refresh();

        Debug.Log($"Icon Forge: Batch exported {batchObjects.Count} icons.");
    }

    private void AddSelectedHierarchyObjectsToBatch()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("Icon Forge: No hierarchy objects selected.");
            return;
        }

        int addedCount = 0;

        foreach (GameObject selectedObject in selectedObjects)
        {
            if (selectedObject == null)
            {
                continue;
            }

            if (batchObjects.Contains(selectedObject))
            {
                continue;
            }

            batchObjects.Add(selectedObject);
            addedCount++;
        }

        Debug.Log($"Icon Forge: Added {addedCount} selected hierarchy objects to batch.");
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