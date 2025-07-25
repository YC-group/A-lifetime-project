using System;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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
    public string PrefabAddress
    {
        get => prefabAddress;
        set => prefabAddress = value;
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

    public PrefabSpawnSave() { }
    public PrefabSpawnSave(string prefabAddress, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        this.prefabAddress = prefabAddress;
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }
    public async Task<GameObject> Spawn(Transform parent = null)
    {
        if (string.IsNullOrEmpty(prefabAddress))
        {
            Debug.LogError("Spawn 失敗: Prefab Address 為空！");
            return null;
        }

        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(prefabAddress, position, rotation, parent);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject instance = handle.Result;
            instance.transform.localScale = scale;
            if (instance.CompareTag("Barrier"))
            {
                instance.layer = LayerMask.NameToLayer("Barrier");
            }
            return instance;
        }
        else
        {
            Debug.LogError($"Spawn 失敗，無法載入 Address: {prefabAddress}");
            return null;
        }
    }
}

