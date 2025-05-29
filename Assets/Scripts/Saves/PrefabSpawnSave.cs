using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
/// <summary>
/// Prefab生成模板(JSON) - js5515
/// </summary>
[Serializable]
public class PrefabSpawnSave
{
    [SerializeField] private string prefabAddress;
    [SerializeField] private Vector3 position = Vector3.zero;
    [SerializeField] private Quaternion rotation = Quaternion.identity;
    [SerializeField] private Vector3 scale = Vector3.one;

    public PrefabSpawnSave(string prefabAddress, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        this.prefabAddress = prefabAddress;
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }
    /*
    public GameObject Spawn(Transform parent = null)
    {
        if (string.IsNullOrEmpty(prefabAddress))
        {
            Debug.LogError("Spawn 失敗: Prefab Address 為空！");
            return null;
        }

        GameObject instance = null;

        try
        {
            // 實例化
            instance = GameObject.Instantiate(prefab, position, rotation, parent);
            instance.transform.localScale = scale;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Spawn 失敗：{ex.Message}");
            return null;
        }

        return instance;
    }
    */
}

