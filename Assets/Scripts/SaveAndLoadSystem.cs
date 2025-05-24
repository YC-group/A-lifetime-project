using UnityEngine;
using System.IO;
using UnityEditor;
using Newtonsoft.Json;
/// <summary>
/// 存讀檔工具 - js5515
/// 存讀JSON檔
/// 存讀asset檔
/// </summary>
public class SaveAndLoadSystem
{
    public static void SaveAsJSON<T>(T t, string path)
    {
        if (t == null)
        {
            Debug.LogError("要儲存的資料為 null");
            return;
        }

        if (!path.EndsWith(".json")) path += ".json";

        Directory.CreateDirectory(Path.GetDirectoryName(path));

        string json = JsonConvert.SerializeObject(t, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public static T LoadFromJSON<T>(string path)
    {
        if (!path.EndsWith(".json")) path += ".json";

        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"載入 JSON 時出錯: {ex.Message}");
                return default(T);
            }
        }
        else
        {
            Debug.LogWarning($"找不到 JSON 檔案！(路徑: {path})");
            return default(T);
        }
    }

#if UNITY_EDITOR
    public static void SaveAsPrefab(GameObject go, string path)
    {
        if(go == null)
        {
            Debug.LogError("要轉存成prefab的GameObject為 null");
            return;
        }

        if (!path.EndsWith(".prefab")) path += ".prefab";

        Directory.CreateDirectory(Path.GetDirectoryName(path));

        // 儲存為 Prefab 並連接
        PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.AutomatedAction);
        Debug.Log($"儲存 Prefab 成功：{path}");
    }

    public static void SaveAsAsset<T>(T t, string path) where T : ScriptableObject
    {
        if(t == null)
        {
            Debug.LogError("要儲存的資料為 null");
            return;
        }

        if (!path.EndsWith(".asset")) path += ".asset";

        Directory.CreateDirectory(Path.GetDirectoryName(path));

        if (AssetDatabase.LoadAssetAtPath<T>(path) == null)
        {
            AssetDatabase.CreateAsset(t, path);
        }
        else
        {
            EditorUtility.SetDirty(t);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static T LoadFromAsset<T>(string path) where T : ScriptableObject
    {
        if (!path.EndsWith(".asset")) path += ".asset";

        T t = AssetDatabase.LoadAssetAtPath<T>(path);
        if (t == null)
        {
            Debug.LogWarning($"找不到 asset 檔案！(路徑: {path})");
        }
        return t;
    }
#endif
}
