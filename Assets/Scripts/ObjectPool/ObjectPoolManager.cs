using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

/// <summary>
/// 物件池腳本 - Jerry0401
/// </summary>
///
public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] private bool addToDontDestroyOnLoad = false; // 決定切換場景時物件池是否重置

    private GameObject emptyHolder; // 多個物件池的父物件
    
    private static GameObject gameObjectEmpty; // GameObject 的物件池

    private static Dictionary<GameObject, ObjectPool<GameObject>> objectPools; // 用於找多個物件池
    private static Dictionary<GameObject, GameObject> cloneToPrfabMap; // 用來查詢已存入物件池的物件字典

    public enum PoolType
    {
        GameObjects,
    }

    public static PoolType PoolingType;

    private void Awake()
    {
        objectPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
        cloneToPrfabMap = new Dictionary<GameObject, GameObject>();
        SetupEmpties(); // 建立物件池
    }
    
    
    private void SetupEmpties()
    {
        emptyHolder = new GameObject("Object pools");
        
        gameObjectEmpty = new GameObject("GameObjects");
        gameObjectEmpty.transform.SetParent(emptyHolder.transform);

        if (addToDontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObjectEmpty.transform.root);
        }
    }
    
    private static void CreatePool(GameObject prefab, Vector3 pos, Quaternion rot,
        PoolType poolType = PoolType.GameObjects)
    {
        // 生成新的物件池
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: ()=> CreateObject(prefab, pos, rot, poolType),
            actionOnGet: OnGetObjects,
            actionOnRelease: OnReleaseObjects,
            actionOnDestroy: OnDestroyObjects
            );
        
        objectPools.Add(prefab, pool); // 加入物件池字典
    }

    private static GameObject CreateObject(GameObject obj, Vector3 pos, Quaternion rot,
        PoolType poolType = PoolType.GameObjects)
    {
        
        // HACK: 如果生成物件是從無到有則要用到 Instantiate
        // prefab.SetActive(false);
        //
        // GameObject obj = Instantiate(prefab, pos, rot);
        //
        // prefab.SetActive(true);
        
        GameObject parentObj = SetParentObject(poolType);
        obj.transform.SetParent(parentObj.transform);
        
        return obj;
    }

    private static void OnGetObjects(GameObject obj)
    {
        
    }

    private static void OnReleaseObjects(GameObject obj)
    {
        obj.SetActive(false);
    }

    private static void OnDestroyObjects(GameObject obj)
    {
        if (cloneToPrfabMap.ContainsKey(obj))
        {
            cloneToPrfabMap.Remove(obj);
        }
    }

    private static GameObject SetParentObject(PoolType poolType)
    {
        // 將物件放入哪個物件池下
        switch (poolType)
        {
            case PoolType.GameObjects:
                return gameObjectEmpty;
            default:
                return null;
        }
    }

    /// <summary>
    /// 將已新增的物件加入物件池
    /// </summary>
    /// <param name="objectToSpawn"></param>
    /// <param name="poolType"></param>
    public static void AddExistObjetToPool(GameObject objectToSpawn, PoolType poolType = PoolType.GameObjects)
    {
        if (!objectPools.ContainsKey(objectToSpawn))
        {
            CreatePool(objectToSpawn, objectToSpawn.transform.position, objectToSpawn.transform.rotation, poolType);
        }
        
        if (!cloneToPrfabMap.ContainsKey(objectToSpawn))
        {
            cloneToPrfabMap.Add(objectToSpawn, objectToSpawn);
        }
    }

    /// <summary>
    /// 將物件放回物件池
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="poolType"></param>
    public static void ReturnObjectToPool(GameObject obj, PoolType poolType = PoolType.GameObjects)
    {
        if (cloneToPrfabMap.TryGetValue(obj, out GameObject prefab))
        {
            GameObject parentObject = SetParentObject(poolType);

            if (obj.transform.parent != parentObject.transform)
            {
                obj.transform.SetParent(parentObject.transform);
            }
            
            if (objectPools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            {
                pool.Release(obj);
            }
        }
        else
        {
            Debug.LogWarning("Trying to return an object is not pooled: " + obj.name);
        }
    }
    
    // TODO: 生成物件時將物件池化
    
    // private static T SpawnObject<T>(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation,
    //     PoolType poolType = PoolType.GameObjects) where T : UnityEngine.Object
    // {
    //     if (!objectPools.ContainsKey(objectToSpawn))
    //     {
    //         CreatePool(objectToSpawn, spawnPos, spawnRotation, poolType);
    //     }
    //     GameObject obj = objectPools[objectToSpawn].Get();
    //
    //     if (obj != null)
    //     {
    //         if (!cloneToPrfabMap.ContainsKey(obj))
    //         {
    //             cloneToPrfabMap.Add(obj, objectToSpawn);
    //         }
    //         
    //         obj.transform.position = spawnPos;
    //         obj.transform.rotation = spawnRotation;
    //         obj.SetActive(true);
    //
    //         if (typeof(T) == typeof(GameObject))
    //         {
    //             return obj as T;
    //         }
    //         
    //         T component = obj.GetComponent<T>();
    //         if (component == null)
    //         {
    //             Debug.LogError($"Object {objectToSpawn.name} doesn't have a component of type {typeof(T)}");
    //             return null;
    //         }
    //
    //         return component;
    //     }
    //     
    //     return null;
    // }
    //
    // public static T SpawnObject<T>(T typePrefab, Vector3 spawnPos, Quaternion spawnRotation,
    //     PoolType poolType = PoolType.GameObjects) where T : Component
    // {
    //     return SpawnObject<T>(typePrefab.gameObject, spawnPos, spawnRotation, poolType);
    // }
    //
    // public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation,
    //     PoolType poolType = PoolType.GameObjects)
    // {
    //     return SpawnObject<GameObject>(objectToSpawn, spawnPos, spawnRotation, poolType);
    // }
}
