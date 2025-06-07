#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Unity.Cinemachine.CinemachineSplineRoll;
/// <summary>
/// 資料轉換器 - js5515
/// </summary>
public static class DataConverter
{        
    public static SpawnData ConvertToSpawnData(SpawnSave spawnSave)
    {
        SpawnData spawnData = new SpawnData();
        RoomData roomData;

        roomData = SaveAndLoadSystem.LoadByName<RoomData>(spawnSave.RoomId);
        if (roomData == null)
        {
            Debug.LogError("無法取得roomData");
            return default;
        }

        spawnData.Spawnpoint = spawnSave.Spawnpoint;
        spawnData.RoomData = roomData;

        return spawnData;
    }

    public static SpawnSave ConvertToSpawnSave(SpawnData spawnData)
    {
        SpawnSave spawnSave = new SpawnSave();
        string roomId;

        roomId = SaveAndLoadSystem.GetAssetFileNameWithoutExtension(spawnData.RoomData);
        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogError("無法取得roomData的檔案名稱");
            return default;
        }

        spawnSave.Spawnpoint = spawnData.Spawnpoint;
        spawnSave.RoomId = roomId;

        return spawnSave;
    }

    public static PrefabSpawnSave ConvertToPrefabSpawnSave(PrefabSpawnData prefabSpawnData)
    {
        PrefabSpawnSave prefabSpawnSave = new PrefabSpawnSave();
        string prefabAddress;

        prefabAddress = SaveAndLoadSystem.GetPrefabAddress(prefabSpawnData.Prefab);
        if (string.IsNullOrEmpty(prefabAddress))
        {
            Debug.LogError("找不到該 Prefab 的 Address");
            return default;
        }

        prefabSpawnSave.PrefabAddress = prefabAddress;
        prefabSpawnSave.Position = prefabSpawnData.Position;
        prefabSpawnSave.Rotation = prefabSpawnData.Rotation;
        prefabSpawnSave.Scale = prefabSpawnData.Scale;

        return prefabSpawnSave;
    }

    public static DoorSave ConvertToDoorSave(DoorData doorData)
    {
        DoorSave doorSave = new DoorSave();
        List<SpawnSave> spawns = new List<SpawnSave>();

        foreach(SpawnData spawn in doorData.Spawns)
        {
            spawns.Add(ConvertToSpawnSave(spawn));
        }

        doorSave.Pss = ConvertToPrefabSpawnSave(doorData.Psd);
        doorSave.Spawns = spawns;

        return doorSave;
    }

    public static RoomSave ConvertToRoomSave(RoomData roomData)
    {
        RoomSave roomSave = new RoomSave();
        List<PrefabSpawnSave> enemies = new List<PrefabSpawnSave>();
        List<PrefabSpawnSave> items = new List<PrefabSpawnSave>();
        List<PrefabSpawnSave> buildings = new List<PrefabSpawnSave>();
        string roomId;

        roomData.Enemies.ForEach(enemy => enemies.Add(ConvertToPrefabSpawnSave(enemy)));
        roomData.Items.ForEach(item => items.Add(ConvertToPrefabSpawnSave(item)));
        roomData.Buildings.ForEach(building => buildings.Add(ConvertToPrefabSpawnSave(building)));

        roomId = SaveAndLoadSystem.GetAssetFileNameWithoutExtension(roomData);
        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogError("無法取得roomData的檔案名稱");
            return default;
        }

        roomSave.Enemies = enemies;
        roomSave.Items = items;
        roomSave.Buildings = buildings;
        roomSave.RoomId = roomId;

        return roomSave;
    }

    public static LevelSave ConvertToLevelSave(LevelData levelData)
    {
        LevelSave levelSave = new LevelSave();
        List<PrefabSpawnSave> barriers = new List<PrefabSpawnSave>();
        List<RoomSave> rooms = new List<RoomSave>();
        List<DoorSave> doors = new List<DoorSave>();
        string startRoomId;

        foreach(PrefabSpawnData barrier in levelData.Barriers)
        {
            barriers.Add(ConvertToPrefabSpawnSave(barrier));
        }
        foreach(RoomData roomData in levelData.Rooms)
        {
            rooms.Add(ConvertToRoomSave(roomData));
        }
        foreach(DoorData doorData in levelData.Doors)
        {
            doors.Add(ConvertToDoorSave(doorData));
        }
        startRoomId = SaveAndLoadSystem.GetAssetFileNameWithoutExtension(levelData.StartRoomData);
        if (string.IsNullOrEmpty(startRoomId))
        {
            Debug.LogError("無法取得roomData的檔案名稱");
            return default;
        }

        levelSave.Barriers = barriers;
        levelSave.Rooms = rooms;
        levelSave.Doors = doors;
        levelSave.StartRoomId = startRoomId;
        levelSave.StartSpawnpoint = levelData.StartSpawnpoint;

        return levelSave;
    }
}
#endif