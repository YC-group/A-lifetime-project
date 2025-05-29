using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
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
            SpawnSave spawnSave = new SpawnSave();
            spawnSave.Spawnpoint = spawnpointObject.transform.position;
            door.AddSpawn(spawnSave);
            spawnpointObject = null;
            EditorUtility.SetDirty(door);
            EditorSceneManager.MarkSceneDirty(door.gameObject.scene);
        }

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}
