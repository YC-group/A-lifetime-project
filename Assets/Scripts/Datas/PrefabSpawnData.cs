using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
/// <summary>
/// Prefab生成模板 - js5515
/// </summary>
[Serializable]
public class PrefabSpawnData
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 position = Vector3.zero;
    [SerializeField] private Quaternion rotation = Quaternion.identity;
    [SerializeField] private Vector3 scale = Vector3.one;
    public GameObject Prefab
    {
        get => prefab;
        set => prefab = value;
    }
    public Vector3 Position
    {
        get => position;
        set => position = value;
    }
    public Quaternion Rotation
    {
        get => rotation;
        set => rotation = value;
    }
    public Vector3 Scale
    {
        get => scale;
        set => scale = value;
    }

    public PrefabSpawnData(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        this.prefab = prefab;
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }

    /// <summary>
    /// 給原物件和原prefab回傳一份PrefabSpawnData
    /// </summary>
    /// <param name="go">原始物件</param>
    /// <param name="prefab">原始prefab</param>
    /// <returns>回傳一份PrefabSpawnData</returns>
    public static PrefabSpawnData MakeData(GameObject go, GameObject prefab)
    {
        Transform transform = go.transform;
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        Vector3 scale = transform.localScale;
        return new PrefabSpawnData(prefab, position, rotation, scale);
    }

    public GameObject Spawn(Transform parent = null)
    {
        // 檢查 prefab 是否為 null
        if (prefab == null)
        {
            Debug.LogError("Spawn 失敗：Prefab 為 null！");
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

}

