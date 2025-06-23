using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// 關卡生成以及房間管理 - js5515
/// </summary>
public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    private string currentRoomId;
    private Vector3 currentSpawnpoint;
    private Dictionary<string, RoomSave> allRooms = new();
    private Dictionary<string, List<string>> roomLinks = new();

    public static RoomManager GetInstance()
    {
        if (Instance == null)
        {
            Instance = GameObject.FindAnyObjectByType<RoomManager>();
        }
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public string GetCurrentRoomId()
    {
        return currentRoomId;
    }

    public void ChangeRoom(string targetRoomId)
    {
        if (allRooms.ContainsKey(targetRoomId))
        {
            currentRoomId = targetRoomId;

            RoomSave roomSave = allRooms[targetRoomId];
        }
        else
        {
            Debug.LogError($"未能找到該房間: {targetRoomId} ，無法變更房間");
            return;
        }
    }


    public void LoadLevel(LevelSave levelSave)
    {
        //設定初始房間和重生點
        currentRoomId = levelSave.StartRoomId;
        currentSpawnpoint = levelSave.StartSpawnpoint;

        List<RoomSave> roomSaves = levelSave.Rooms;
        foreach (RoomSave roomSave in roomSaves)
        {
            allRooms.Add(roomSave.RoomId, roomSave);
        }

        List<DoorSave> doorSaves = levelSave.Doors;
        foreach (DoorSave door in doorSaves)
        {
            string roomA = door.Spawns[0].RoomId;
            string roomB = door.Spawns[1].RoomId;

            if (!roomLinks.ContainsKey(roomA)) roomLinks[roomA] = new();
            if (!roomLinks.ContainsKey(roomB)) roomLinks[roomB] = new();

            // 雙向連接
            if (!roomLinks[roomA].Contains(roomB)) roomLinks[roomA].Add(roomB);
            if (!roomLinks[roomB].Contains(roomA)) roomLinks[roomB].Add(roomA);
        }


        // TODO: 生成barrier和door，玩家出生
    }

#if UNITY_EDITOR
    public void LoadLevel(LevelData levelData)
    {
        LoadLevel(DataConverter.ConvertToLevelSave(levelData));
    }
#endif
}
