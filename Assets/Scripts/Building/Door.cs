using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 門的邏輯 - js5515
/// </summary>
public class Door : MonoBehaviour
{
    [SerializeField] private List<SpawnData> spawns;

    public List<SpawnData> Spawns
    {
        get => spawns;
        set => spawns = value;
    }
}
