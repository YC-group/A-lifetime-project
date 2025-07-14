using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class FloodFillTools
{
    public static List<Vector3Int> FourDirections = new List<Vector3Int>()
    {
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.forward,
        Vector3Int.back
    };

    public static List<Vector3Int> SixDirections = new List<Vector3Int>() 
    {
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.forward,
        Vector3Int.back,
        Vector3Int.up,
        Vector3Int.down
    };

    public static HashSet<Vector3Int> FloodFill3D(
        Vector3Int start,
        HashSet<Vector3Int> visited,
        List<Vector3Int> directions,
        Func<Vector3Int, bool> IsValid
    )
    {
        HashSet<Vector3Int> result = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            if (visited.Contains(current)) continue;
            if (!IsValid(current)) continue;

            visited.Add(current);
            result.Add(current);

            foreach (Vector3Int direction in directions)
            {
                Vector3Int next = current + direction;
                if (!visited.Contains(next) && IsValid(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return result;
    }

    public static List<HashSet<Vector3Int>> FindAllConnectedRegions3D(
        HashSet<Vector3Int> visited,
        List<Vector3Int> directions,
        Func<Vector3Int, GameObject> func,
        Vector3Int boundsStart,
        Vector3Int boundsEnd
    )
    {
        List<HashSet<Vector3Int>> result = new List<HashSet<Vector3Int>>();

        for (int i = boundsStart.x; i <= boundsEnd.x; ++i)
        {
            for(int j = boundsStart.y; j <= boundsEnd.y; ++j)
            {
                for (int k = boundsStart.z; k <= boundsEnd.z; ++k)
                {
                    Vector3Int current = new Vector3Int(i, j, k);

                    if (visited.Contains(current)) continue;

                    GameObject target = func(current);
                    if(target == null) continue;

                    Func<Vector3Int, bool> IsValid = pos => CommonTools.IsInBounds(pos, boundsStart, boundsEnd) && CommonTools.IsSamePrefab(func(pos), target);
                    var region = FloodFill3D(current, visited, directions, IsValid);

                    if (region.Count == 0) continue;
                    result.Add(region);
                }
            }
        }

        return result;
    }
}
