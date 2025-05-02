using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Codice.Client.BaseCommands;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 關卡存讀檔流程 - js5515
/// </summary>

public class LevelSaveAndLoadEditorWindow : EditorWindow
{
    #region 變數
    private Vector2 scrollPos; //滾動視窗參數

    private int selectedTab = 0; //選擇tab的參數
    private string[] tabNames = { "設定", "存讀檔" }; //選擇tab的選項

    private string levelName;

    private Grid buildingGrid; //建築用網格

    private Vector3Int detectBoundsStart = new Vector3Int(0, 1, 0); //房間偵測範圍起點
    private Vector3Int detectBoundsEnd = new Vector3Int(9, 3, 9); //房間偵測範圍終點
    private Vector3 gridOffset = new Vector3(0f, 0.5f, 0f); //網格顯示偏移值

    private Color roomColor = new Color(0f, 1f, 0f, 0.25f); //偵測房間顏色
    private Color detectBoundsColor = new Color(1f, 0f, 0f, 1f); //偵測範圍顏色
    private Color roomConnectionLineColor = new Color(0.5f, 0f, 1f, 1f); //房間連結線顏色
    private float roomConnectionLineThickness = 10f; //房間連結線粗細

    private HashSet<Vector3Int> barrierPositions = new HashSet<Vector3Int>(); //所有barrier的網格座標
    private HashSet<Vector3Int> doorPositions = new HashSet<Vector3Int>(); //所有door的網格座標
    private List<HashSet<Vector3Int>> detectedRooms = new List<HashSet<Vector3Int>>(); //所有房間內部網格座標的清單
    private Dictionary<int, List<int>> roomConnections = new Dictionary<int, List<int>>(); //所有房間之間的連結

    private List<GameObject> barrierList = new List<GameObject>();
    private List<GameObject> doorList = new List<GameObject>();
    #endregion

    [MenuItem("Tools/Level Save And Load")]
    public static void ShowWindow()
    {
        GetWindow<LevelSaveAndLoadEditorWindow>("Level Save And Load");
    }

    private void OnEnable()
    {
        if (buildingGrid == null) buildingGrid = GameObject.FindWithTag("BuildingGrid")?.GetComponent<Grid>();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (detectedRooms == null) return;

        //房間顯示
        for (int i = 0; i < detectedRooms.Count; i++)
        {
            var room = detectedRooms[i];

            // 畫出每個房間格子
            foreach (var pos in room)
            {
                Vector3 worldPos = buildingGrid.GetCellCenterWorld(pos) + Vector3.Scale(gridOffset, buildingGrid.cellSize);
                Handles.color = roomColor;
                Handles.CubeHandleCap(0, worldPos, Quaternion.identity, Mathf.Max(buildingGrid.cellSize.x, buildingGrid.cellSize.z), EventType.Repaint);
            }

            // 加上房間編號標籤
            Vector3 roomCenter = GetRoomCenter(room);
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 20;
            style.normal.textColor = Color.yellow;
            Handles.Label(roomCenter, $"Room {i}", style);
        }


        //房間偵測範圍顯示
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        Handles.color = detectBoundsColor;
        Vector3 min = buildingGrid.CellToWorld(detectBoundsStart) + Vector3.Scale(gridOffset, buildingGrid.cellSize);
        Vector3 max = buildingGrid.CellToWorld(detectBoundsEnd) + Vector3.Scale(gridOffset, buildingGrid.cellSize);
        Vector3 size = max - min + buildingGrid.cellSize;
        Handles.DrawWireCube(min + size / 2f, size);
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

        //房間連結顯示
        foreach (var kvp in roomConnections)
        {
            int roomAIndex = kvp.Key;
            Vector3 centerA = GetRoomCenter(detectedRooms[roomAIndex]);

            foreach (int roomBIndex in kvp.Value)
            {
                if (roomBIndex > roomAIndex)
                {
                    Vector3 centerB = GetRoomCenter(detectedRooms[roomBIndex]);
                    Handles.color = roomConnectionLineColor;
                    Handles.DrawAAPolyLine(roomConnectionLineThickness, new Vector3[] { centerA, centerB });
                }
            }
        }
    }

    private void OnGUI()
    {
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

        switch (selectedTab)
        {
            case 0:
                SettingsTabGUI();
                break;
            case 1:
                SaveAndLoadTabGUI();
                break;
        }
    }

    private void SettingsTabGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));

        buildingGrid = (Grid)EditorGUILayout.ObjectField("Building Grid", buildingGrid, typeof(Grid), true);

        detectBoundsStart = EditorGUILayout.Vector3IntField("Grid Start", detectBoundsStart);
        detectBoundsEnd = EditorGUILayout.Vector3IntField("Grid End", detectBoundsEnd);
        gridOffset = EditorGUILayout.Vector3Field("Visual Offset", gridOffset);

        EditorGUILayout.Space(10);

        roomColor = EditorGUILayout.ColorField("Room Color", roomColor);
        detectBoundsColor = EditorGUILayout.ColorField("Bounds Color", detectBoundsColor);
        roomConnectionLineColor = EditorGUILayout.ColorField("Connection Line Color", roomConnectionLineColor);
        roomConnectionLineThickness = EditorGUILayout.Slider("Connection Line Thickness", roomConnectionLineThickness, 0.1f, 10f);

        EditorGUILayout.EndScrollView();
    }

    private void SaveAndLoadTabGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));

        levelName = EditorGUILayout.TextField("Level Name", levelName);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Clear All Detect Data"))
        {
            ClearAllData();
        }

        if (GUILayout.Button("Collect Wall & Door Positions from Scene"))
        {
            CollectBarrierAndDoorPositions();
        }

        if (GUILayout.Button("Detect Rooms"))
        {
            DetectRooms();
        }

        if (GUILayout.Button("Save Level"))
        {
            SaveLevel();
        }

        GUILayout.Label($"Detected {detectedRooms.Count} room(s).", EditorStyles.boldLabel);

        EditorGUILayout.EndScrollView();
    }

    private void ClearAllData()
    {
        barrierPositions.Clear();
        doorPositions.Clear();
        detectedRooms.Clear();
        roomConnections.Clear();
    }

    //取得所有barrier和door
    private void CollectBarrierAndDoorPositions()
    {
        //重置位置資料
        barrierList.Clear();
        doorList.Clear();
        barrierPositions.Clear();
        doorPositions.Clear();
        //取得所有物件
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        //走訪所有物件
        foreach (GameObject go in allObjects)
        {
            //轉換物件座標
            Vector3Int pos = buildingGrid.WorldToCell(go.transform.position);
            //若不在偵測範圍內則略過
            if (!IsInBounds(pos)) continue;

            if (go.CompareTag("Barrier"))
            {
                barrierPositions.Add(pos);

                barrierList.Add(go);
            }
            else if (go.CompareTag("Door"))
            {
                doorPositions.Add(pos);
                barrierPositions.Add(pos);

                doorList.Add(go);
            }
        }

        Debug.Log($"收集到 {barrierPositions.Count} 個邊界格子 (包含 {doorPositions.Count} 個門) (範圍 {detectBoundsStart} ~ {detectBoundsEnd})。\n");
        Debug.Log($"Barrier個數: {barrierList.Count}, Door個數: {doorList.Count}");
    }

    //找出所有房間
    private void DetectRooms()
    {
        //重置所有房間和房間連結
        detectedRooms.Clear();
        roomConnections.Clear();
        //用於紀錄拜訪過的位置
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        //走訪偵測範圍
        for (int x = detectBoundsStart.x; x <= detectBoundsEnd.x; x++)
        {
            for (int y = detectBoundsStart.y; y <= detectBoundsEnd.y; y++)
            {
                for (int z = detectBoundsStart.z; z <= detectBoundsEnd.z; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);

                    //若該格沒有被拜訪且不是屏障，使用FloodFill找出連通空間
                    if (!visited.Contains(pos) && !barrierPositions.Contains(pos))
                    {
                        var room = FloodFill(pos, visited);
                        //確認房間是封閉的才採用
                        if (IsRoomEnclosed(room))
                        {
                            detectedRooms.Add(room);
                        }
                    }
                }
            }
        }

        Debug.Log($"總共偵測到 {detectedRooms.Count} 間房間。\n");

        BuildDoorBasedRoomConnections();
    }

    //偵測由門所連結的房間
    private void BuildDoorBasedRoomConnections()
    {
        roomConnections.Clear();

        Dictionary<Vector3Int, int> cellToRoom = new Dictionary<Vector3Int, int>();
        for (int i = 0; i < detectedRooms.Count; i++)
        {
            foreach (var cell in detectedRooms[i])
            {
                cellToRoom[cell] = i;
            }
        }

        foreach (var doorPos in doorPositions)
        {
            HashSet<int> connectedRooms = new HashSet<int>();

            foreach (var dir in GetFourDirections())
            {
                Vector3Int neighbor = doorPos + dir;
                if (cellToRoom.TryGetValue(neighbor, out int roomIndex))
                {
                    connectedRooms.Add(roomIndex);
                }
            }

            int[] roomArray = new int[connectedRooms.Count];
            connectedRooms.CopyTo(roomArray);

            for (int i = 0; i < roomArray.Length; i++)
            {
                for (int j = i + 1; j < roomArray.Length; j++)
                {
                    if (!roomConnections.ContainsKey(roomArray[i]))
                        roomConnections[roomArray[i]] = new List<int>();
                    if (!roomConnections[roomArray[i]].Contains(roomArray[j]))
                        roomConnections[roomArray[i]].Add(roomArray[j]);

                    if (!roomConnections.ContainsKey(roomArray[j]))
                        roomConnections[roomArray[j]] = new List<int>();
                    if (!roomConnections[roomArray[j]].Contains(roomArray[i]))
                        roomConnections[roomArray[j]].Add(roomArray[i]);
                }
            }
        }
    }

    /// <summary>
    /// 檢查關卡名稱是否在 Assets/Levels 下已存在資料夾。
    /// </summary>
    /// <param name="levelName">要檢查的關卡名稱</param>
    /// <returns>若名稱重複則回傳 true，否則為 false</returns>
    public bool IsLevelNameDuplicate(string levelName)
    {
        if (string.IsNullOrEmpty(levelName))
        {
            Debug.LogError("關卡名稱為空!");
            return true;
        }

        string levelsPath = "Assets/Levels";
        if (!Directory.Exists(levelsPath))
        {
            Debug.LogError("Levels資料夾不存在");
            return true;
        }

        string[] subFolders = Directory.GetDirectories(levelsPath, "*", SearchOption.TopDirectoryOnly);
        foreach (string subFolder in subFolders)
        {
            string folderName = Path.GetFileName(subFolder);
            if (levelName.Equals(folderName))
            {
                Debug.LogError("關卡名稱重複!");
                return true;
            }
        }

        return false;
    }

    //取得物件原本的prefab
    private GameObject GetPrefab(GameObject go)
    {
        return PrefabUtility.GetCorrespondingObjectFromSource(go);
    }


    //關卡保存
    private void SaveLevel()
    {
        if(IsLevelNameDuplicate(levelName)) return;

        //創建LevelData
        LevelData levelData = ScriptableObject.CreateInstance<LevelData>();

        //barrier資料處理
        List<PrefabSpawnData> barriers = new List<PrefabSpawnData>();
        foreach (GameObject barrier in barrierList)
        {
            GameObject prefab = GetPrefab(barrier);
            if (prefab != null)
            {
                PrefabSpawnData prefabSpawnData = PrefabSpawnData.MakeData(barrier, prefab);

                barriers.Add(prefabSpawnData);
            }
            else
            {
                Debug.LogError("無法取得barrier原先的prefab");
                return;
            }
        }
        //將barriers和doors存好
        levelData.Barriers = barriers;
        levelData.Doors = SaveDoorData();

        //房間部分處理
        int roomCount = 0;
        List<RoomData> rooms = new List<RoomData>();
        foreach (HashSet<Vector3Int> room in detectedRooms)
        {
            //創建RoomData
            RoomData roomData = ScriptableObject.CreateInstance<RoomData>();

            //取得房間邊界
            Vector3Int min;
            Vector3Int max;
            GetRoomBounds(room, out min, out max);

            //邊界轉世界座標，並且做偏移
            Vector3 minPos = min;
            Vector3 maxPos = max;
            float halfCellSize = buildingGrid.cellSize.x / 2;
            minPos += new Vector3(-halfCellSize, halfCellSize, -halfCellSize);
            maxPos += new Vector3(halfCellSize, 3*halfCellSize, halfCellSize);

            //找到房間內所有的物件
            List<GameObject> foundObjects = FindObjectsInAreaWithoutCollider(minPos, maxPos);
            Debug.Log($"Room {roomCount} 有 {foundObjects.Count} 個物件");

            //找到的物件處理
            List<PrefabSpawnData> enemies = new List<PrefabSpawnData>();
            List<PrefabSpawnData> items = new List<PrefabSpawnData>();
            List<PrefabSpawnData> buildings = new List<PrefabSpawnData>();
            bool isSetSpawnpoint = false;
            foreach (GameObject go in foundObjects)
            {
                Vector3Int cellPos = buildingGrid.WorldToCell(go.transform.position);
                if (!room.Contains(cellPos)) continue; // 不屬於房間，略過

                if (go.CompareTag("Enemy"))
                {
                    PrefabSpawnData prefabSpawnData = PrefabSpawnData.MakeData(go, GetPrefab(go));
                    enemies.Add(prefabSpawnData);
                }
                else if (go.CompareTag("Item"))
                {
                    PrefabSpawnData prefabSpawnData = PrefabSpawnData.MakeData(go, GetPrefab(go));
                    items.Add(prefabSpawnData);
                }
                else if (go.CompareTag("Building"))
                {
                    PrefabSpawnData prefabSpawnData = PrefabSpawnData.MakeData(go, GetPrefab(go));
                    buildings.Add(prefabSpawnData);
                }
                else if (go.CompareTag("Spawnpoint"))
                {
                    if(isSetSpawnpoint) //已經設定過重生點
                    {
                        Debug.LogError("單一房間擁有複數重生點");

                        EditorUtility.DisplayDialog(
                            "儲存失敗",
                            $"房間 {roomCount} 擁有多個 Spawnpoint，請確認每個房間僅有一個重生點。",
                            "我知道了"
                        );
                        return;
                    }
                    else 
                    {
                        roomData.Spawnpoint = go.transform.position;
                        isSetSpawnpoint = true;
                    }
                }
                else
                {
                    //不是任何指定tag的物件處理(預計不做任何處理，頂多提醒開發者)
                    //PrefabSpawnData prefabSpawnData = PrefabSpawnData.MakeData(go, GetPrefab(go));


                }
            }

            if (!isSetSpawnpoint)
            {
                Debug.LogError("單一房間未擁有重生點");

                EditorUtility.DisplayDialog(
                    "儲存失敗",
                    $"房間 {roomCount} 未擁有 Spawnpoint，請確認每個房間都有一個重生點。",
                    "我知道了"
                );
                return;
            }

            if(enemies.Count != 0) roomData.Enemies = enemies;
            if(items.Count != 0) roomData.Items = items;
            if(buildings.Count != 0) roomData.Buildings = buildings;
            
            //roomData寫成asset檔
            string roomDataPath = $"Assets/Levels/{levelName}/RoomDatas/{levelName}_room_{roomCount}.asset";
            SaveAndLoadSystem.SaveAsAsset<RoomData>(roomData, roomDataPath);

            // 使用儲存後的 asset 實例來填入 levelData
            RoomData savedRoomData = AssetDatabase.LoadAssetAtPath<RoomData>(roomDataPath);
            rooms.Add(savedRoomData);

            ++roomCount;
        }

        levelData.Rooms = rooms;

        //levelData寫成asset檔
        string levelDataPath = $"Assets/Levels/{levelName}/{levelName}.asset";
        SaveAndLoadSystem.SaveAsAsset<LevelData>(levelData, levelDataPath);

    }

    //取得指定空間的物件(無須碰撞箱)
    public List<GameObject> FindObjectsInAreaWithoutCollider(Vector3 pointA, Vector3 pointB)
    {
        List<GameObject> foundObjects = new List<GameObject>();

        Vector3 min = Vector3.Min(pointA, pointB);
        Vector3 max = Vector3.Max(pointA, pointB);

        // 搜尋場景裡所有活著的 GameObject
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (var obj in allObjects)
        {
            // 只搜尋啟用中的物件
            if (!obj.activeInHierarchy) continue;

            Vector3 pos = obj.transform.position;

            if (pos.x >= min.x && pos.x <= max.x &&
                pos.y >= min.y && pos.y <= max.y &&
                pos.z >= min.z && pos.z <= max.z)
            {
                foundObjects.Add(obj);
            }
        }

        return foundObjects;
    }

    //取得指定空間的物件
    public List<GameObject> FindObjectsInArea(Vector3 pointA, Vector3 pointB, LayerMask? layerMask = null)
    {
        // 計算中心點與範圍
        Vector3 center = (pointA + pointB) / 2f;
        Vector3 size = new Vector3(
            Mathf.Abs(pointA.x - pointB.x),
            Mathf.Abs(pointA.y - pointB.y),
            Mathf.Abs(pointA.z - pointB.z)
        );

        // 避免大小為0
        //size += Vector3.one * 0.01f;

        // 決定實際用的 LayerMask
        int maskToUse = layerMask.HasValue ? layerMask.Value : Physics.DefaultRaycastLayers;

        // 使用 OverlapBox 搜尋範圍內所有碰撞體
        Collider[] colliders = Physics.OverlapBox(center, size / 2f, Quaternion.identity, maskToUse);

        List<GameObject> foundObjects = new List<GameObject>();

        foreach (var collider in colliders)
        {
            foundObjects.Add(collider.gameObject);
        }

        return foundObjects;
    }

    //取得房間最小和最大角落
    public void GetRoomBounds(HashSet<Vector3Int> room, out Vector3Int min, out Vector3Int max)
    {
        if (room == null || room.Count == 0)
        {
            Debug.LogError("房間是空的！");
            min = max = Vector3Int.zero;
            return;
        }

        // 初始值設成第一個格子
        using (var enumerator = room.GetEnumerator())
        {
            enumerator.MoveNext();
            min = max = enumerator.Current;
        }

        foreach (var cell in room)
        {
            if (cell.x < min.x) min.x = cell.x;
            if (cell.y < min.y) min.y = cell.y;
            if (cell.z < min.z) min.z = cell.z;

            if (cell.x > max.x) max.x = cell.x;
            if (cell.y > max.y) max.y = cell.y;
            if (cell.z > max.z) max.z = cell.z;
        }
    }


    //存所有門的資料
    private List<DoorData> SaveDoorData()
    {
        List<DoorData> doorDatas = new List<DoorData>();

        // 建立 cell 對房間的對應表
        Dictionary<Vector3Int, int> cellToRoom = new Dictionary<Vector3Int, int>();
        for (int i = 0; i < detectedRooms.Count; i++)
        {
            foreach (var cell in detectedRooms[i])
            {
                cellToRoom[cell] = i;
            }
        }

        foreach (var doorPos in doorPositions)
        {
            HashSet<int> connectedRooms = new HashSet<int>();

            foreach (var dir in GetFourDirections())
            {
                Vector3Int neighbor = doorPos + dir;
                if (cellToRoom.TryGetValue(neighbor, out int roomIndex))
                {
                    connectedRooms.Add(roomIndex);
                }
            }

            int[] roomArray = new int[connectedRooms.Count];
            connectedRooms.CopyTo(roomArray);

            //以下是保存 Door 資料
            Vector3 worldPos = buildingGrid.GetCellCenterWorld(doorPos);
            Collider[] hits = Physics.OverlapSphere(worldPos, 0.1f);
            GameObject doorObject = null;

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Door"))
                {
                    doorObject = hit.gameObject;
                    break;
                }
            }

            if (doorObject == null)
                continue;

            Undo.RecordObject(doorObject, "Assign Door Component");

            Door door = doorObject.GetComponent<Door>();
            if (door == null)
            {
                door = Undo.AddComponent<Door>(doorObject);
            }
            else
            {
                Undo.RecordObject(door, "Modify Door Links");
            }

            string roomName1 = $"{levelName}_room_{roomArray[0]}";
            string roomName2 = $"{levelName}_room_{roomArray[1]}";
            PrefabSpawnData psd = PrefabSpawnData.MakeData(doorObject, GetPrefab(doorObject));
            DoorData doorData = new DoorData(psd, roomName1, roomName2);

            doorDatas.Add(doorData);

            door.SetRoomName(doorData);

            EditorUtility.SetDirty(doorObject);
            EditorUtility.SetDirty(door);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(doorObject.scene);
        }

        //Debug.Log("Door Data Saved");

        return doorDatas;
    }

    //取得房間中心
    private Vector3 GetRoomCenter(HashSet<Vector3Int> room)
    {
        Vector3 sum = Vector3.zero;
        foreach (var cell in room)
        {
            sum += buildingGrid.GetCellCenterWorld(cell) + Vector3.Scale(gridOffset, buildingGrid.cellSize);
        }
        return sum / room.Count;
    }

    //FloodFill演算法，用來找連通空間
    private HashSet<Vector3Int> FloodFill(Vector3Int start, HashSet<Vector3Int> visited)
    {
        HashSet<Vector3Int> region = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            if (visited.Contains(current) || barrierPositions.Contains(current)) continue;
            if (!IsInBounds(current)) continue;

            visited.Add(current);
            region.Add(current);

            foreach (Vector3Int dir in GetSixDirections())
            {
                Vector3Int next = current + dir;
                if (!visited.Contains(next) && !barrierPositions.Contains(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return region;
    }

    //判斷房間是否封閉
    private bool IsRoomEnclosed(HashSet<Vector3Int> region)
    {
        foreach (var pos in region)
        {
            foreach (Vector3Int dir in GetFourDirections())
            {
                Vector3Int neighbor = pos + dir;
                if (!region.Contains(neighbor) && !barrierPositions.Contains(neighbor))
                {
                    return false;
                }
                if (!IsInBounds(neighbor))
                {
                    return false;
                }
            }
        }
        return true;
    }

    //判斷給定座標是否在偵測範圍
    private bool IsInBounds(Vector3Int pos)
    {
        return pos.x >= detectBoundsStart.x && pos.x <= detectBoundsEnd.x &&
               pos.y >= detectBoundsStart.y && pos.y <= detectBoundsEnd.y &&
               pos.z >= detectBoundsStart.z && pos.z <= detectBoundsEnd.z;
    }

    //6方向，上下左右前後
    private List<Vector3Int> GetSixDirections()
    {
        return new List<Vector3Int>
        {
            Vector3Int.right,
            Vector3Int.left,
            Vector3Int.forward,
            Vector3Int.back,
            Vector3Int.up,
            Vector3Int.down
        };
    }

    //4方向，左右前後
    private List<Vector3Int> GetFourDirections()
    {
        return new List<Vector3Int>
        {
            Vector3Int.right,
            Vector3Int.left,
            Vector3Int.forward,
            Vector3Int.back
        };
    }
}
