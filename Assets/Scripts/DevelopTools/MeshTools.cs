using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshTools
{

    public static GameObject CombineGameObjects(GameObject[] gameObjects)
    {
        if (gameObjects.Length == 0)
        {
            Debug.LogError("無法合併gameObject: 物件數量為0");
            return null;
        }

        Mesh mesh = CombineMeshes(gameObjects);

        GameObject gameObject = new GameObject();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshFilter.sharedMesh = mesh;
        if (gameObjects.Length > 0)
        {
            var originalRenderer = gameObjects[0].GetComponentInChildren<MeshRenderer>();
            if (originalRenderer != null)
            {
                meshRenderer.sharedMaterials = originalRenderer.sharedMaterials;
            }
            else
            {
                // 使用 Unity 預設材質
                meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
            }
        }

        return gameObject;
    }

    public static Mesh CombineMeshes(GameObject[] gameObjects)
    {
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        List<CombineInstance> combineInstances = new List<CombineInstance>();

        foreach (GameObject gameObject in gameObjects)
        {
            meshFilters.AddRange(gameObject.GetComponentsInChildren<MeshFilter>());
        }

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh == null) continue;

            CombineInstance combineInstance = new CombineInstance();
            combineInstance.mesh = meshFilter.sharedMesh;
            combineInstance.transform = meshFilter.transform.localToWorldMatrix;

            combineInstances.Add(combineInstance);
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.CombineMeshes(combineInstances.ToArray());
        return mesh;
    }
}
