using System.Collections.Generic;
using System;
using UnityEngine;
/// <summary>
/// 門的資料 - js5515
/// </summary>
[Serializable]
public class DoorData
{

    [SerializeField] private PrefabSpawnData psd;
    [SerializeField] private List<SpawnData> spawns;
    public PrefabSpawnData Psd
    {
        get => psd;
        set => psd = value;
    }
    public List<SpawnData> Spawns
    {
        get => spawns;
        set => spawns = value;
    }

    public DoorData(PrefabSpawnData psd, List<SpawnData> spawns)
    {
        this.psd = psd;
        this.spawns = spawns;
    }
}
