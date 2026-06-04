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

        if (GUILayout.Button("Generate Preview"))
        {
            Debug.Log($"Icon Forge preview requested for: {sourceObject.name}");
        }

        GUI.enabled = true;
    }
}