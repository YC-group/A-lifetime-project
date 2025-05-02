using System.Threading;
using UnityEngine;
/// <summary>
/// Prefab生成模板 - js5515
/// </summary>
[System.Serializable]
public class PrefabSpawnData
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 position = Vector3.zero;
    [SerializeField] private Quaternion rotation = Quaternion.identity;
    [SerializeField] private Vector3 scale = Vector3.one;

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
}

