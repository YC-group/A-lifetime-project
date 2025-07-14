using UnityEngine;
using System.IO;
using UnityEditor;
using Newtonsoft.Json;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Settings;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
/// <summary>
/// 存讀檔工具 - js5515
/// 存讀JSON檔
/// 存讀asset檔
/// </summary>
public static class SaveAndLoadTools
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

        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        string json = JsonConvert.SerializeObject(t, Formatting.Indented, settings);
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

    public static async Task<T> LoadFromAddressable<T>(string address) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogError("從 Addressable 載入失敗: address 為空");
            return null;
        }

        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        else
        {
            Debug.LogError($"從 Addressable 載入失敗: {address}");
            return null;
        }
    }

    public static async Task<T> LoadFromAddressableAndInstantiate<T>(string address) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogError("載入失敗：Address 為空");
            return null;
        }

        // 只能對 GameObject 使用 InstantiateAsync
        if (typeof(T) != typeof(GameObject))
        {
            Debug.LogError("InstantiateAsync 只支援 GameObject 類型");
            return null;
        }

        var handle = Addressables.InstantiateAsync(address);
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"實例化失敗：{address}");
            return null;
        }

        return handle.Result as T;
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

    public static void SaveAsAsset<T>(T t, string path) where T : UnityEngine.Object
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

    public static T LoadFromAsset<T>(string path) where T : UnityEngine.Object
    {
        if (!path.EndsWith(".asset")) path += ".asset";

        T t = AssetDatabase.LoadAssetAtPath<T>(path);
        if (t == null)
        {
            Debug.LogWarning($"找不到 asset 檔案！(路徑: {path})");
        }
        return t;
    }

    public static T LoadByName<T>(string fileNameWithoutExtension) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string assetName = Path.GetFileNameWithoutExtension(path);

            if (assetName == fileNameWithoutExtension)
            {
                return LoadFromAsset<T>(path);
            }
        }

        Debug.LogWarning($"找不到名稱為 {fileNameWithoutExtension} 的 {typeof(T).Name}");
        return null;
    }

    public static string GetAssetFileName(Object asset)
    {
        if (asset == null)
            return null;

        // 取得資源的完整路徑，例如 "Assets/Data/Rooms/Room_01.asset"
        string path = AssetDatabase.GetAssetPath(asset);
        if (string.IsNullOrEmpty(path))
            return null;

        // 只取檔名部分，例如 "Room_01.asset"
        return Path.GetFileName(path);
    }

    public static string GetAssetFileNameWithoutExtension(Object asset)
    {
        string filename = GetAssetFileName(asset);
        if (filename == null) return null;

        // 去掉副檔名，例如 "Room_01"
        return Path.GetFileNameWithoutExtension(filename);
    }

    public static string GetPrefabAddress(GameObject prefab)
    {
        if (prefab == null)
            return null;

        string assetPath = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(assetPath))
            return null;

        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("無法取得 Addressable 設定，請確認已啟用 Addressables。");
            return null;
        }

        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        var entry = settings.FindAssetEntry(guid);
        if (entry != null)
        {
            return entry.address;
        }

        return null;
    }

    public static void AddPrefabToAddressables(string prefabPath, string address, string groupName = "Default Local Group")
    {
        // 加載 prefab
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError("找不到 prefab: " + prefabPath);
            return;
        }

        // 取得 Addressables 設定
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("找不到 AddressableAssetSettings");
            return;
        }

        // 尋找或建立 group
        AddressableAssetGroup group = settings.FindGroup(groupName);
        if (group == null)
        {
            group = settings.CreateGroup(groupName, false, false, false, null, typeof(BundledAssetGroupSchema));
        }

        // 加入 prefab
        string assetGUID = AssetDatabase.AssetPathToGUID(prefabPath);
        AddressableAssetEntry entry = settings.CreateOrMoveEntry(assetGUID, group);
        entry.address = address;

        // 儲存變更
        AssetDatabase.SaveAssets();
        Debug.Log($"成功將 {prefab.name} 加入 Addressables，Address 為 {address}，Group 為 {groupName}");
    }

    public static void AddPrefabToAddressables(GameObject prefab, string address, string groupName = "Default Local Group")
    {
        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        AddPrefabToAddressables(prefabPath, address, groupName);
    }
#endif
}
