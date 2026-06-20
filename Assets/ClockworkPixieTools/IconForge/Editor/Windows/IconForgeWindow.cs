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

    private Texture2D logoTexture;
    private Texture2D footprintsTexture;
    private GameObject sourceObject;
    private IconForgeProfile profile;
    private IconOutputSettings outputSettings;
    private Texture2D previewTexture;
    private float iconFillPercent = 70f;
    private bool showDebugSection = false;
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
    private Vector2 batchListScrollPosition;
    private IconCameraPreset cameraPreset = IconCameraPreset.Isometric;
    private IconItemTypePreset itemTypePreset = IconItemTypePreset.Generic;
    private IconLightingProfile lightingProfile = IconLightingProfile.Neutral;
    private IconForgeProfile activeProfile;
    private Vector3 objectRotationOffset = Vector3.zero;
    private Vector2 objectCompositionOffset = Vector2.zero;
    private Vector2 settingsScrollPosition;

    private void OnEnable()
    {
        logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/ClockworkPixieTools/IconForge/Editor/UI/Textures/Logo_Symbol.png");

        footprintsTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/ClockworkPixieTools/IconForge/Editor/UI/Textures/Footprints.png");
    }

    private void OnGUI()
    {
        DrawBackgroundBranding();

        float fixedBottomHeight = 480f;

        settingsScrollPosition = EditorGUILayout.BeginScrollView(
            settingsScrollPosition,
            GUILayout.Height(position.height - fixedBottomHeight));

        DrawSourceSection();
        DrawProfileSection();
        DrawBatchSection();
        DrawOutputSection();
        DrawRenderingSection();
        DrawDebugSection();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(6);

        DrawPreviewSection();
        DrawExportSection();
        DrawFooterLogo();
    }

    private void DrawSourceSection()
    {
        EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        sourceObject = (GameObject)EditorGUILayout.ObjectField(
            "Source Object",
            sourceObject,
            typeof(GameObject),
            true);

        if (EditorGUI.EndChangeCheck())
        {
            GeneratePreview();
        }

        EditorGUILayout.HelpBox(
            "Select a prefab or scene object to prepare for icon generation.",
            MessageType.Info);

        EditorGUILayout.Space(10);
    }

    private void DrawProfileSection()
    {
        EditorGUILayout.LabelField("Profile", EditorStyles.boldLabel);

        activeProfile = (IconForgeProfile)EditorGUILayout.ObjectField(
            "Profile",
            activeProfile,
            typeof(IconForgeProfile),
            false);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Load Profile"))
        {
            LoadProfile();
        }

        if (GUILayout.Button("Save Current Settings To Profile"))
        {
            SaveCurrentSettingsToProfile();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
    }

    private void DrawBatchSection()
    {
        EditorGUILayout.LabelField(
            "Batch Processing",
            EditorStyles.boldLabel);

        EditorGUILayout.LabelField($"Batch Count: {batchObjects.Count}");

        if (GUILayout.Button("Add Source Object To Batch"))
        {
            if (sourceObject != null &&
                !batchObjects.Contains(sourceObject))
            {
                batchObjects.Add(sourceObject);
            }
        }

        if (GUILayout.Button("Add Selected Project Assets To Batch"))
        {
            AddSelectedProjectAssetsToBatch();
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear Batch"))
        {
            batchObjects.Clear();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        if (batchObjects.Count > 0)
        {
            EditorGUILayout.BeginVertical("box");

            for (int i = 0; i < batchObjects.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                batchObjects[i] = (GameObject)EditorGUILayout.ObjectField(
                    batchObjects[i],
                    typeof(GameObject),
                    false);

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    batchObjects.RemoveAt(i);
                    i--;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(10);
    }

    private void DrawDebugSection()
    {
        showDebugSection = EditorGUILayout.Foldout(
            showDebugSection,
            "Debug",
            true);

        if (!showDebugSection)
        {
            EditorGUILayout.Space(5);
            return;
        }

        if (GUILayout.Button("Create Debug Rig"))
        {
            if (sourceObject == null)
            {
                Debug.LogWarning("Icon Forge: No source object selected.");
                return;
            }

            IconForgeDebugRig.CreateDebugRig(
                sourceObject,
                iconFillPercent / 100f,
                backgroundColor,
                cameraPreset,
                lightingProfile,
                objectRotationOffset,
                objectCompositionOffset);
        }

        EditorGUILayout.Space(10);
    }

    private void DrawPreviewSection()
    {
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

        if (previewTexture == null)
        {
            EditorGUILayout.HelpBox(
                "No preview generated yet.",
                MessageType.Info);

            return;
        }

        EditorGUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        DrawTexturePreview(
            previewTexture,
            "Detail Preview",
            256);

        GUILayout.Space(12);

        DrawTexturePreview(
            previewTexture,
            "64 x 64",
            64);

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
    }

    private void DrawExportSection()
    {
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

        previewTexture = IconRenderCapture.GeneratePreview(
            sourceObject,
            iconFillPercent / 100f,
            256,
            renderBackgroundColor,
            cameraPreset,
            lightingProfile,
            objectRotationOffset,
            objectCompositionOffset);

        Repaint();
    }

    private string ExportPreviewPng()
    {
        if (sourceObject == null)
        {
            Debug.LogWarning("Icon Forge: No source object selected for export.");
            return null;
        }

        int exportResolution = resolutionValues[selectedResolutionIndex];

        Color renderBackgroundColor = useTransparentBackground ? new Color(0f, 0f, 0f, 0f) : backgroundColor;

        Texture2D exportTexture = IconRenderCapture.GeneratePreview(
            sourceObject,
            iconFillPercent / 100f,
            exportResolution,
            renderBackgroundColor,
            cameraPreset,
            lightingProfile,
            objectRotationOffset,
            objectCompositionOffset);

        if (exportTexture == null)
        {
            Debug.LogWarning("Icon Forge: Could not generate export texture.");
            return null;
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

        string baseFileName = IconFileNamingUtility.GetSafeFileName(sourceObject.name);

        string fullPath = IconFileNamingUtility.GetUniqueFilePath(
            exportFolder,
            baseFileName,
            "png");

        byte[] pngData = exportTexture.EncodeToPNG();
        File.WriteAllBytes(fullPath, pngData);

        AssetDatabase.Refresh();

        Debug.Log($"Icon Forge: Exported {exportResolution}x{exportResolution} PNG to {fullPath}");

        return fullPath;
    }

    private void ExportBatchPngs()
    {
        if (batchObjects.Count == 0)
        {
            Debug.LogWarning("Icon Forge: Batch list is empty.");
            return;
        }

        List<string> exportedIconPaths = new List<string>();

        foreach (GameObject batchObject in batchObjects)
        {
            if (batchObject == null)
            {
                continue;
            }

            sourceObject = batchObject;

            string exportedPath = ExportPreviewPng();

            if (!string.IsNullOrEmpty(exportedPath))
            {
                exportedIconPaths.Add(exportedPath);
            }
        }

        if (exportedIconPaths.Count > 0)
        {
            string exportFolder = "Assets/GeneratedIcons";

            if (outputSettings != null && !string.IsNullOrWhiteSpace(outputSettings.exportFolder))
            {
                exportFolder = outputSettings.exportFolder;
            }

            int exportResolution = resolutionValues[selectedResolutionIndex];

            IconContactSheetUtility.CreateContactSheet(
                exportedIconPaths,
                exportFolder,
                exportResolution);
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

    private void DrawOutputSection()
    {
        EditorGUILayout.LabelField("Output Settings", EditorStyles.boldLabel);

        outputSettings = (IconOutputSettings)EditorGUILayout.ObjectField(
            "Output Settings",
            outputSettings,
            typeof(IconOutputSettings),
            false);

        EditorGUI.BeginChangeCheck();

        useTransparentBackground = EditorGUILayout.Toggle(
            "Transparent Background",
            useTransparentBackground);

        if (!useTransparentBackground)
        {
            backgroundColor = EditorGUILayout.ColorField(
                "Background Color",
                backgroundColor);
        }

        selectedResolutionIndex = EditorGUILayout.Popup(
            "Export Resolution",
            selectedResolutionIndex,
            resolutionLabels);

        if (EditorGUI.EndChangeCheck())
        {
            GeneratePreview();
        }

        EditorGUILayout.Space(10);
    }

    private void DrawRenderingSection()
    {
        EditorGUILayout.LabelField("Rendering & Composition", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        itemTypePreset = (IconItemTypePreset)EditorGUILayout.EnumPopup(
            "Item Type",
            itemTypePreset);

        if (EditorGUI.EndChangeCheck())
        {
            ApplyItemTypePreset();
            GeneratePreview();
        }

        EditorGUI.BeginChangeCheck();

        cameraPreset = (IconCameraPreset)EditorGUILayout.EnumPopup(
            "Camera Preset",
            cameraPreset);

        lightingProfile = (IconLightingProfile)EditorGUILayout.EnumPopup(
            "Lighting Profile",
            lightingProfile);

        iconFillPercent = EditorGUILayout.Slider(
            "Icon Fill",
            iconFillPercent,
            10f,
            175f);

        objectRotationOffset.x = EditorGUILayout.Slider(
            "Rotation X",
            objectRotationOffset.x,
            -180f,
            180f);

        objectRotationOffset.y = EditorGUILayout.Slider(
            "Rotation Y",
            objectRotationOffset.y,
            -180f,
            180f);

        objectRotationOffset.z = EditorGUILayout.Slider(
            "Rotation Z",
            objectRotationOffset.z,
            -180f,
            180f);

        if (GUILayout.Button("Reset Rotation"))
        {
            objectRotationOffset = Vector3.zero;
            GeneratePreview();
        }

        objectCompositionOffset.x = EditorGUILayout.Slider(
            "Offset X",
            objectCompositionOffset.x,
            -1f,
            1f);

        objectCompositionOffset.y = EditorGUILayout.Slider(
            "Offset Y",
            objectCompositionOffset.y,
            -1f,
            1f);

        if (GUILayout.Button("Reset Offset"))
        {
            objectCompositionOffset = Vector2.zero;
            GeneratePreview();
        }

        if (EditorGUI.EndChangeCheck())
        {
            GeneratePreview();
        }

        EditorGUILayout.Space(10);
    }

    private void DrawTexturePreview(
    Texture2D texture,
    string label,
    int size)
    {
        EditorGUILayout.BeginVertical();

        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
        labelStyle.alignment = TextAnchor.MiddleCenter;

        GUILayout.Label(label, labelStyle, GUILayout.Width(size));

        Rect previewRect = GUILayoutUtility.GetRect(
            size,
            size,
            GUILayout.Width(size),
            GUILayout.Height(size));

        if (useTransparentBackground)
        {
            DrawCheckerboard(previewRect);
        }

        if (texture != null)
        {
            GUI.DrawTexture(
                previewRect,
                texture,
                ScaleMode.ScaleToFit,
                true);
        }

        EditorGUILayout.EndVertical();
    }

    private void ApplyItemTypePreset()
    {
        switch (itemTypePreset)
        {
            case IconItemTypePreset.Generic:
                cameraPreset = IconCameraPreset.Isometric;
                iconFillPercent = 70f;
                objectRotationOffset = Vector3.zero;
                objectCompositionOffset = Vector2.zero;
                break;

            case IconItemTypePreset.Weapon:
                cameraPreset = IconCameraPreset.Isometric;
                iconFillPercent = 80f;
                objectRotationOffset = new Vector3(-62f, 0f, 0f);
                objectCompositionOffset = Vector2.zero;
                break;

            case IconItemTypePreset.HeadFocus:
                cameraPreset = IconCameraPreset.Isometric;
                iconFillPercent = 175f;
                objectRotationOffset = new Vector3(-62f, 0f, 0f);
                objectCompositionOffset = new Vector2(0.3f, -0.3f);
                break;

            case IconItemTypePreset.Shield:
                cameraPreset = IconCameraPreset.Front;
                iconFillPercent = 80f;
                objectRotationOffset = Vector3.zero;
                objectCompositionOffset = Vector2.zero;
                break;

            case IconItemTypePreset.Potion:
                cameraPreset = IconCameraPreset.Isometric;
                iconFillPercent = 75f;
                objectRotationOffset = Vector3.zero;
                objectCompositionOffset = Vector2.zero;
                break;

            case IconItemTypePreset.Chest:
                cameraPreset = IconCameraPreset.Isometric;
                iconFillPercent = 80f;
                objectRotationOffset = Vector3.zero;
                objectCompositionOffset = Vector2.zero;
                break;

            case IconItemTypePreset.Resource:
                cameraPreset = IconCameraPreset.Isometric;
                iconFillPercent = 75f;
                objectRotationOffset = Vector3.zero;
                objectCompositionOffset = Vector2.zero;
                break;
        }
    }

    private void LoadProfile()
    {
        if (activeProfile == null)
        {
            return;
        }

        itemTypePreset = activeProfile.itemType;

        cameraPreset = activeProfile.cameraPreset;

        lightingProfile = activeProfile.lightingProfile;

        iconFillPercent = activeProfile.fillPercent;

        selectedResolutionIndex = 0;

        for (int i = 0; i < resolutionValues.Length; i++)
        {
            if (resolutionValues[i] == activeProfile.resolution)
            {
                selectedResolutionIndex = i;
                break;
            }
        }

        useTransparentBackground =
            activeProfile.transparentBackground;

        backgroundColor =
            activeProfile.backgroundColor;

        GeneratePreview();

        Repaint();
    }

    private void SaveCurrentSettingsToProfile()
    {
        if (activeProfile == null)
        {
            Debug.LogWarning("Icon Forge: No profile selected to save to.");
            return;
        }

        activeProfile.itemType = itemTypePreset;
        activeProfile.cameraPreset = cameraPreset;
        activeProfile.lightingProfile = lightingProfile;
        activeProfile.fillPercent = iconFillPercent;
        activeProfile.resolution = resolutionValues[selectedResolutionIndex];
        activeProfile.transparentBackground = useTransparentBackground;
        activeProfile.backgroundColor = backgroundColor;

        EditorUtility.SetDirty(activeProfile);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Icon Forge: Saved settings to profile '{activeProfile.name}'.");
    }

    private void AddSelectedProjectAssetsToBatch()
    {
        Object[] selectedAssets = Selection.objects;

        if (selectedAssets == null || selectedAssets.Length == 0)
        {
            Debug.LogWarning("Icon Forge: No project assets selected.");
            return;
        }

        int addedCount = 0;

        foreach (Object selectedAsset in selectedAssets)
        {
            if (selectedAsset == null)
            {
                continue;
            }

            string assetPath = AssetDatabase.GetAssetPath(selectedAsset);
            Debug.Log($"Icon Forge Debug | Name: {selectedAsset.name} | Path: {assetPath}");

            if (string.IsNullOrEmpty(assetPath))
            {
                continue;
            }

            if (AssetDatabase.IsValidFolder(assetPath))
            {
                addedCount += AddGameObjectsFromFolder(assetPath);
                Debug.Log($"Icon Forge Debug | Folder detected: {assetPath}");
                continue;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab != null)
            {
                if (!batchObjects.Contains(prefab))
                {
                    batchObjects.Add(prefab);
                    addedCount++;
                }
            }
        }

        Debug.Log($"Icon Forge: Added {addedCount} project assets to batch.");
    }

    private int AddGameObjectsFromFolder(string folderPath)
    {
        int addedCount = 0;

        string[] assetGuids = AssetDatabase.FindAssets(
            "t:GameObject",
            new[] { folderPath });

        foreach (string assetGuid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);

            GameObject assetObject =
                AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (assetObject == null)
            {
                continue;
            }

            if (batchObjects.Contains(assetObject))
            {
                continue;
            }

            batchObjects.Add(assetObject);
            addedCount++;
        }

        return addedCount;
    }

    private void DrawBackgroundBranding()
    {
        if (footprintsTexture == null)
        {
            return;
        }

        float footprintWidth = 220f;
        float footprintHeight = 440f;

        Rect footprintRect = new Rect(
            position.width - footprintWidth - 12f,
            position.height - footprintHeight - 12f,
            footprintWidth,
            footprintHeight);

        Color previousColor = GUI.color;

        GUI.color = new Color(1f, 1f, 1f, 0.10f);

        GUI.DrawTexture(
            footprintRect,
            footprintsTexture,
            ScaleMode.ScaleToFit,
            true);

        GUI.color = previousColor;
    }

    private void DrawHeader()
    {
        GUILayout.Space(6);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;

        GUILayout.Label(
            "Clockwork Pixie Icon Forge",
            titleStyle);
    }

    private void DrawFooterLogo()
    {
        if (logoTexture == null)
        {
            return;
        }

        Rect logoRect = GUILayoutUtility.GetRect(
            120,
            120,
            GUILayout.ExpandWidth(true));

        logoRect.width = 120;
        logoRect.height = 120;
        logoRect.x = (position.width - logoRect.width) * 0.5f;

        Color previousColor = GUI.color;

        GUI.color = new Color(
            1f,
            1f,
            1f,
            0.65f);

        GUI.DrawTexture(
            logoRect,
            logoTexture,
            ScaleMode.ScaleToFit,
            true);

        GUI.color = previousColor;

        GUILayout.Space(6);
    }
}