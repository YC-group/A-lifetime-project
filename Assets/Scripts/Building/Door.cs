using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 門的邏輯 - js5515
/// </summary>
public class Door : MonoBehaviour
{
    [SerializeField] private List<SpawnSave> spawns;

    public void SetSpawns(List<SpawnData> spawnDatas)
    {
        foreach (SpawnData spawnData in spawnDatas)
        {
            this.spawns.Add(DataConverter.ConvertToSpawnSave(spawnData));
        }
    }

    public void SetSpawns(List<SpawnSave> spawnSaves)
    {
        this.spawns = spawnSaves;
    }

    public void AddSpawn(SpawnSave spawnSave)
    {
        this.spawns.Add(spawnSave);
    }

#if UNITY_EDITOR
        
    public List<SpawnData> GetSpawnDatas()
    {
        List<SpawnData> spawnDatas = new List<SpawnData>();

        foreach(SpawnSave spawnSave in this.spawns)
        {
            spawnDatas.Add(DataConverter.ConvertToSpawnData(spawnSave));
        }

        return spawnDatas;
    }
#endif
}
