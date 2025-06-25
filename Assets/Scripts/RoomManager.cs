using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// 關卡生成以及房間管理 - js5515
/// </summary>
public class RoomManager : MonoBehaviour
{
    private static RoomManager Instance;

    private string currentRoomId;
    private Vector3 currentSpawnpoint;
    private Dictionary<string, RoomSave> allRooms = new();
    private Dictionary<string, bool> isAlert = new();
    private Dictionary<string, List<string>> roomLinks = new();

    public static RoomManager GetInstance()
    {
        if (Instance == null)
        {
            Instance = GameObject.FindAnyObjectByType<RoomManager>();
            if (Instance == null)
            {
                Debug.LogError("找不到 RoomManager，請確認場景中是否有啟用的 RoomManager 物件");
                return null;
            }
        }
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("已存在其他 RoomManager 實例，將刪除此物件。");
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

    public void LoadRoom(string targetRoomId)
    {

    }

    public void PlayerSpawn()
    {
        
    }

    public void LoadLevel(LevelSave levelSave)
    {
        //設定初始房間和重生點
        currentRoomId = levelSave.StartRoomId;
        currentSpawnpoint = levelSave.StartSpawnpoint;

        //設定所有房間
        List<RoomSave> roomSaves = levelSave.Rooms;
        foreach (RoomSave roomSave in roomSaves)
        {
            allRooms.Add(roomSave.RoomId, roomSave);
            isAlert.Add(roomSave.RoomId, false);
        }

        //建立房間之間的連結
        List<DoorSave> doorSaves = levelSave.Doors;
        foreach (DoorSave doorSave in doorSaves)
        {
            string roomA = doorSave.Spawns[0].RoomId;
            string roomB = doorSave.Spawns[1].RoomId;

            if (!roomLinks.ContainsKey(roomA)) roomLinks[roomA] = new();
            if (!roomLinks.ContainsKey(roomB)) roomLinks[roomB] = new();

            // 雙向連接
            if (!roomLinks[roomA].Contains(roomB)) roomLinks[roomA].Add(roomB);
            if (!roomLinks[roomB].Contains(roomA)) roomLinks[roomB].Add(roomA);

            //door生成
            doorSave.Pss.Spawn(instance =>
            {
                if(instance == null)
                {
                    Debug.Log("載入關卡失敗: door生成失敗");
                    return;
                }

                //設定door重生點
                Door door = instance.GetComponent<Door>();
                if (door == null)
                {
                    Debug.LogError("載入關卡失敗: 未能取得 Door component");
                    return;
                }
                instance.GetComponent<Door>().SetSpawns(doorSave.Spawns);
            });
        }

        //barrier生成
        List<PrefabSpawnSave> barriers = levelSave.Barriers;
        foreach(PrefabSpawnSave barrier in barriers)
        {
            barrier.Spawn();
        }

        //玩家生成
    }

#if UNITY_EDITOR
    public void LoadLevel(LevelData levelData)
    {
        LoadLevel(DataConverter.ConvertToLevelSave(levelData));
    }
#endif
}
