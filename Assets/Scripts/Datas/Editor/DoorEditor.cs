using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(Door))]
public class DoorEditor : Editor
{
    private Door door;
    private GameObject spawnpointObject;

    private void OnEnable()
    {
        door = (Door)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        spawnpointObject = (GameObject)EditorGUILayout.ObjectField("新增重生點物件在這裡", spawnpointObject, typeof(GameObject), true);
        if (spawnpointObject != null)
        {
            SpawnData spawnData = new SpawnData();
            spawnData.Spawnpoint = spawnpointObject.transform.position;
            door.Spawns.Add(spawnData);
            spawnpointObject = null;
        }

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}
