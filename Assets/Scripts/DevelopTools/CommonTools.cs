using UnityEditor;
using UnityEngine;

public static class CommonTools
{
    public static bool IsInBounds(Vector3Int pos, Vector3Int min, Vector3Int max)
    {
        return pos.x >= min.x && pos.x <= max.x &&
               pos.y >= min.y && pos.y <= max.y &&
               pos.z >= min.z && pos.z <= max.z;
    }

    public static bool IsSamePrefab(GameObject a, GameObject b)
    {
        return GetPrefab(a) == GetPrefab(b);
    }

    public static GameObject GetPrefab(GameObject gameObject)
    {
        if(gameObject == null) return null;
        return PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
    }
}
