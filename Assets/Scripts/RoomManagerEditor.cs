using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomManager))]
public class RoomManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.TextField("Current Room ID", RoomManager.GetInstance().GetCurrentRoomId());
        EditorGUILayout.Vector3Field("Current Spanwpoint", RoomManager.GetInstance().GetCurrentSpawnpoint());
        GUI.enabled = true;

        if(GUILayout.Button("Clear All"))
        {
            RoomManager.GetInstance().ClearAll();
        }

        base.OnInspectorGUI();
    }
}
