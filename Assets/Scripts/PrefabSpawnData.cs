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
}

