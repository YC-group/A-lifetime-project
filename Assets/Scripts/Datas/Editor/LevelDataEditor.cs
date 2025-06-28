using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private SerializedProperty startSpawnpoint;
    private GameObject startSpawnpointObject;

    private void OnEnable()
    {
        startSpawnpoint = serializedObject.FindProperty("startSpawnpoint");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        startSpawnpointObject = (GameObject)EditorGUILayout.ObjectField("新增起始的重生點物件在這裡", startSpawnpointObject, typeof(GameObject), true);
        if (startSpawnpointObject != null)
        {
            startSpawnpoint.vector3Value = startSpawnpointObject.transform.position;
            startSpawnpointObject = null;
        }

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}
