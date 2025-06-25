using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 門的邏輯 - js5515
/// </summary>
public class Door : MonoBehaviour
{
    [SerializeField] private List<SpawnSave> spawns;
       

    public void SetSpawns(List<SpawnSave> spawnSaves)
    {
        this.spawns = spawnSaves;
    }

    public void AddSpawn(SpawnSave spawnSave)
    {
        this.spawns.Add(spawnSave);
    }

    public void OpenDoor()
    {
        string currentRoomId = RoomManager.GetInstance().GetCurrentRoomId();
        if (string.IsNullOrEmpty(currentRoomId))
        {
            Debug.LogError("開門失敗，未能取得目前房間id");
            return;
        }
        string targetRoomId;
        if (currentRoomId.Equals(spawns[0].RoomId))
        {
            targetRoomId = spawns[1].RoomId;
        }
        else
        {
            targetRoomId = spawns[0].RoomId;
        }

        RoomManager.GetInstance().ChangeRoom(targetRoomId);
    }

#if UNITY_EDITOR
    public void SetSpawns(List<SpawnData> spawnDatas)
    {
        foreach (SpawnData spawnData in spawnDatas)
        {
            this.spawns.Add(DataConverter.ConvertToSpawnSave(spawnData));
        }
    }    
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
