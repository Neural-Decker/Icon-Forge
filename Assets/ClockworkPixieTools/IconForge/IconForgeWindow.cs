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

    private void OnGUI()
    {
        GUILayout.Label("Clockwork Pixie Icon Forge", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        sourceObject = (GameObject)EditorGUILayout.ObjectField(
            "Source Object",
            sourceObject,
            typeof(GameObject),
            true);

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "Select a prefab or scene object to prepare for icon generation.",
            MessageType.Info);

        GUI.enabled = sourceObject != null;

        GUI.enabled = true;

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


        //Peview section
        GUILayout.Label("Preview", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        Rect previewRect = GUILayoutUtility.GetRect(
            256,
            256,
            GUILayout.Width(256),
            GUILayout.Height(256));

        EditorGUI.DrawRect(previewRect, new Color(0.12f, 0.12f, 0.12f));
        GUI.Label(previewRect, "Preview Area", EditorStyles.centeredGreyMiniLabel);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        //Button to Generate preview
        if (GUILayout.Button("Generate Preview"))
        {
            Debug.Log($"Icon Forge preview requested for: {sourceObject.name}");
        }
    }
}