using UnityEditor;
using UnityEngine;

public class IconForgeWindow : EditorWindow
{
    [MenuItem("Tools/Clockwork Pixie/Icon Forge")]
    public static void ShowWindow()
    {
        GetWindow<IconForgeWindow>("Icon Forge");
    }

    private void OnGUI()
    {
        GUILayout.Label("Clockwork Pixie Icon Forge", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        GUILayout.Label("Prototype Build");

        EditorGUILayout.HelpBox(
            "Icon generation functionality not yet implemented.",
            MessageType.Info);
    }
}