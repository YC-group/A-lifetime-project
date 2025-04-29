using UnityEngine;
using System.IO;
/// <summary>
/// 存讀檔工具 - js5515
/// </summary>
public class SaveAndLoadSystem
{
    public static void SaveAsJSON<T>(T t, string path)
    {
        string json = JsonUtility.ToJson(t, true); // 第二個參數 true = 格式化排版
        File.WriteAllText(path, json);
    }

    public static T LoadFromJSON<T>(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<T>(json);
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
}
