using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelEditor))]
public class LevelEditorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        LevelEditor levelEditor = (LevelEditor)target;

        GUILayout.Label("Building Grid Settings", EditorStyles.boldLabel);
        levelEditor.buildingGrid = (Grid)EditorGUILayout.ObjectField("Building Grid", levelEditor.buildingGrid, typeof(Grid), true);
        levelEditor.buildingGridColor = EditorGUILayout.ColorField("Building Grid Color", levelEditor.buildingGridColor);
        levelEditor.buildingGridSize = EditorGUILayout.IntField("Building Grid Size", levelEditor.buildingGridSize);

        GUILayout.Space(10);

        GUILayout.Label("Move Grid Settings", EditorStyles.boldLabel);
        levelEditor.moveGrid = (Grid)EditorGUILayout.ObjectField("Move Grid", levelEditor.moveGrid, typeof(Grid), true);
        levelEditor.moveGridColor = EditorGUILayout.ColorField("Move Grid Color", levelEditor.moveGridColor);
        levelEditor.moveGridSize = EditorGUILayout.IntField("Move Grid Size", levelEditor.moveGridSize);

        GUILayout.Space(10);

        if (GUILayout.Button("Refresh Grid"))
        {
            SceneView.RepaintAll();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
