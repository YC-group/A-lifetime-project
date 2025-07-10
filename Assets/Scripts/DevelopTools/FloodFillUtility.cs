using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class FloodFillUtility
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

    public static HashSet<Vector3Int> FloodFill3D(Vector3Int start, HashSet<Vector3Int> visited, List<Vector3Int> directions, Func<Vector3Int, bool> IsValid)
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
                if (!visited.Contains(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return result;
    }
}
