using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshTools
{
    public static GameObject CombineMeshes(GameObject[] objects, string combinedName, string meshAssetPath)
    {
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        foreach (var go in objects)
        {
            meshFilters.AddRange(go.GetComponentsInChildren<MeshFilter>());
        }

        List<CombineInstance> combine = new List<CombineInstance>();
        foreach (var mf in meshFilters)
        {
            if (mf.sharedMesh == null) continue;

            combine.Add(new CombineInstance
            {
                mesh = mf.sharedMesh,
                transform = mf.transform.localToWorldMatrix
            });
        }

        if (combine.Count == 0)
        {
            Debug.LogWarning("沒有可用的 Mesh 進行合併！");
            return null;
        }

        Mesh combinedMesh = new Mesh
        {
            name = combinedName,
            indexFormat = IndexFormat.UInt32 // 支援超過 65535 點的 mesh
        };
        combinedMesh.CombineMeshes(combine.ToArray());

        // 儲存 Mesh 為 Asset（如果不存在）
        if (!AssetDatabase.Contains(combinedMesh))
        {
            AssetDatabase.CreateAsset(combinedMesh, meshAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        GameObject combinedObject = new GameObject(combinedName);
        var mfCombined = combinedObject.AddComponent<MeshFilter>();
        var mrCombined = combinedObject.AddComponent<MeshRenderer>();

        mfCombined.sharedMesh = combinedMesh;

        // 嘗試從第一個 renderer 複製材質（單一材質情境）
        var firstRenderer = meshFilters[0].GetComponent<MeshRenderer>();
        if (firstRenderer != null)
        {
            mrCombined.sharedMaterials = firstRenderer.sharedMaterials;
        }

        return combinedObject;
    }
}
