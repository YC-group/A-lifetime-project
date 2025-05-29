using System.IO;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 資料轉換器 - js5515
/// </summary>
public static class DataConverter
{        
    public static SpawnData ConvertToSpawnData(SpawnSave spawnSave)
    {
        SpawnData spawnData = new SpawnData();
        spawnData.Spawnpoint = spawnSave.Spawnpoint;
        spawnData.RoomData = SaveAndLoadSystem.LoadByName<RoomData>(spawnSave.RoomId);

        return spawnData;
    }

    public static SpawnSave ConvertToSpawnSave(SpawnData spawnData)
    {
        SpawnSave spawnSave = new SpawnSave();
        spawnSave.Spawnpoint = spawnData.Spawnpoint;
        spawnSave.RoomId = SaveAndLoadSystem.GetAssetFileNameWithoutExtension(spawnData.RoomData);

        return spawnSave;
    }
}
