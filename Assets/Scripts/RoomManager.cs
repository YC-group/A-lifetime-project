using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
/// <summary>
/// 關卡生成以及房間管理 - js5515
/// </summary>
public class RoomManager : MonoBehaviour
{
    private static RoomManager Instance;

    private const string PLAYER_ADDRESS = "TestPlayer";

    private string currentRoomId;
    private Vector3 currentSpawnpoint;
    private Dictionary<string, RoomSave> allRooms = new();
    private Dictionary<string, bool> roomIsAlert = new();
    private Dictionary<string, bool> roomIsVisited = new();
    private Dictionary<string, List<string>> roomLinks = new();

    public string GetCurrentRoomId()
    {
        return currentRoomId;
    }

    public Vector3 GetCurrentSpawnpoint()
    {
        return currentSpawnpoint;
    }

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

    public void ClearAll()
    {
        currentRoomId = null;
        currentSpawnpoint = Vector3.zero;
        allRooms.Clear();
        roomIsAlert.Clear();
        roomIsVisited.Clear();
        roomLinks.Clear();
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

    public async Task LoadRoom(string targetRoomId)
    {
        if (!allRooms.TryGetValue(targetRoomId, out RoomSave targetRoom) || targetRoom == null)
        {
            Debug.LogError("房間無法載入: 無法取得目標房間");
            return;
        }

        try
        {
            var enemyTasks = targetRoom.Enemies?.Select(enemy => enemy.Spawn()) ?? Enumerable.Empty<Task<GameObject>>();
            var itemTasks = targetRoom.Items?.Select(item => item.Spawn()) ?? Enumerable.Empty<Task<GameObject>>();
            var buildingTasks = targetRoom.Buildings?.Select(building => building.Spawn()) ?? Enumerable.Empty<Task<GameObject>>();

            await Task.WhenAll(enemyTasks.Concat(itemTasks).Concat(buildingTasks));
        }
        catch (Exception ex)
        {
            Debug.LogError($"載入房間時發生錯誤: {ex.Message}");
        }
    }

    public void CheckNearRoom()
    {
        List<string> neighbors = roomLinks[currentRoomId];

        foreach(string neighbor in neighbors)
        {

        }
    }

    public async Task PlayerSpawn()
    {
        if (string.IsNullOrEmpty(PLAYER_ADDRESS))
        {
            Debug.LogError("玩家生成失敗: PLAYER_ADDRESS 為空");
            return;
        }

        GameObject player = await SaveAndLoadSystem.LoadFromAddressableAndInstantiate<GameObject>(PLAYER_ADDRESS);
        player.transform.position = currentSpawnpoint;
    }


    public void PlayerRespawn()
    {
        GameObject player = PlayerScript.GetInstance().gameObject;
        if (player == null)
        {
            Debug.LogError("玩家無法重生: 無法取得玩家物件");
            return;
        }

        player.transform.position = currentSpawnpoint;
    }

    public async Task LoadLevel(LevelSave levelSave)
    {
        ClearAll();

        //設定初始房間和重生點
        currentRoomId = levelSave.StartRoomId;
        currentSpawnpoint = levelSave.StartSpawnpoint;

        //設定所有房間
        List<RoomSave> roomSaves = levelSave.Rooms;
        foreach (RoomSave roomSave in roomSaves)
        {
            allRooms.Add(roomSave.RoomId, roomSave);
            roomIsAlert.Add(roomSave.RoomId, false);
            roomIsVisited.Add(roomSave.RoomId, false);
        }
        roomIsVisited[currentRoomId] = true;

        

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
        }

        //barrier生成
        List<PrefabSpawnSave> barriers = levelSave.Barriers;
        await Task.WhenAll(barriers.Select(barrier => barrier.Spawn()));
        Debug.Log("barrier生成完畢!");
        
        //door生成
        foreach(DoorSave doorSave in doorSaves)
        {
            
            GameObject instance = await doorSave.Pss.Spawn();

            if (instance == null)
            {
                Debug.Log("載入關卡失敗: door生成失敗");
                return;
            }

            //設定door重生點
            if (!instance.TryGetComponent<Door>(out Door door))
            {
                Debug.LogError("載入關卡失敗：未能取得 Door component");
                return;
            }
            door.SetSpawns(doorSave.Spawns);
        }
        Debug.Log("door生成完畢!");

        

        //開始房間生成
        await LoadRoom(currentRoomId);

        //玩家生成
        await PlayerSpawn();

        Debug.Log("關卡載入完畢!");
    }

#if UNITY_EDITOR
    public async void LoadLevel(LevelData levelData)
    {
        await LoadLevel(DataConverter.ConvertToLevelSave(levelData));
    }
#endif
}
