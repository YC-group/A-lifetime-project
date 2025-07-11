using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class FloodFillTools
{
    public static List<Vector3Int> fourDirections = new List<Vector3Int>()
    {
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.forward,
        Vector3Int.back
    };

    public static List<Vector3Int> sixDirections = new List<Vector3Int>() 
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
        Func<Vector3Int, bool> IsValid,
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
                    Vector3Int pos = new Vector3Int(i, j, k);

                    if (visited.Contains(pos)) continue;

                    var region = FloodFill3D(pos, visited, directions, IsValid);
                    result.Add(region);
                }
            }
        }

        return result;
    }

    public static bool IsInBounds(Vector3Int pos, Vector3Int min, Vector3Int max)
    {
        return pos.x >= min.x && pos.x <= max.x &&
               pos.y >= min.y && pos.y <= max.y &&
               pos.z >= min.z && pos.z <= max.z;
    }


}
