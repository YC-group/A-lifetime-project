using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomManager))]
public class RoomManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.TextField("Current Room ID", RoomManager.GetInstance().GetCurrentRoomId());
        GUI.enabled = true;

        base.OnInspectorGUI();
    }
}
