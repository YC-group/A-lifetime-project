using UnityEngine;
using UnityEditor;
using System.IO;
/// <summary>
/// 創建asset檔的工具 - js5515
/// </summary>
public static class AssetCreator
{
    // 創建或更新指定資料，並自訂檔名
    public static bool CreateOrUpdateAsset<T>(string path, string fileName) where T : ScriptableObject
    {
        // 檢查檔案名稱
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("檔案名稱不能為空！");
            return false;
        }

        if (!fileName.EndsWith(".asset"))
        {
            fileName += ".asset";  // 自動補上 .asset 副檔名
        }

        // 確保路徑使用正確的分隔符，無論平台如何
        path = path.Replace("\\", "/");
        string fullPath = Path.Combine(path, fileName).Replace("\\", "/");

        // 確保資料夾存在
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }

        // 檢查檔案是否已經存在
        T existingAsset = AssetDatabase.LoadAssetAtPath<T>(fullPath);
        if (existingAsset != null)
        {
            // 已存在，更新資源
            EditorUtility.SetDirty(existingAsset); // 標記為已更改
            Debug.Log($"已更新現有的 Asset：{fullPath}");
            return false;  // 回傳 false，表示沒有創建新檔案
        }

        // 不存在，創建新的資源
        T newAsset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(newAsset, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 聚焦到新的 asset
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newAsset;

        Debug.Log($"成功創建 {typeof(T).Name} 到 {fullPath}");

        return true;  // 回傳 true，表示創建了新檔案
    }
}
