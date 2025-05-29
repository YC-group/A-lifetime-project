using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 門的資料(JSON) - js5515
/// </summary>
[Serializable]
public class DoorSave
{

    [SerializeField] private PrefabSpawnSave pss;
    [SerializeField] private List<SpawnSave> spawns;
    public PrefabSpawnSave Pss
    {
        get => pss;
        set => pss = value;
    }
    public List<SpawnSave> Spawns
    {
        get => spawns;
        set => spawns = value;
    }

    public DoorSave(PrefabSpawnSave pss, List<SpawnSave> spawns)
    {
        this.pss = pss;
        this.spawns = spawns;
    }
}
