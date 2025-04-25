using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 關卡存讀檔流程 - js5515
/// </summary>

public class LevelSaveAndLoadEditorWindow : EditorWindow
{
    #region 變數
    private Vector2 scrollPos; //滾動視窗參數

    private Grid buildingGrid; //建築用網格

    private Vector3Int detectBoundsStart = new Vector3Int(0, 1, 0); //房間偵測範圍起點
    private Vector3Int detectBoundsEnd = new Vector3Int(10, 3, 10); //房間偵測範圍終點
    private Vector3 gridOffset = new Vector3(0f, 0.5f, 0f); //網格顯示偏移值

    private Color roomColor = new Color(0f, 1f, 0f, 0.25f); //偵測房間顏色
    private Color detectBoundsColor = new Color(1f, 0f, 0f, 1f); //偵測範圍顏色
    private Color roomConnectionLineColor = new Color(0.5f, 0f, 1f, 1f); //房間連結線顏色
    private float roomConnectionLineThickness = 10f; //房間連結線粗細

    private HashSet<Vector3Int> barrierPositions = new HashSet<Vector3Int>(); //所有barrier的網格座標
    private HashSet<Vector3Int> doorPositions = new HashSet<Vector3Int>(); //所有door的網格座標
    private List<HashSet<Vector3Int>> detectedRooms = new List<HashSet<Vector3Int>>(); //所有房間內部網格座標的清單
    private Dictionary<int, List<int>> roomConnections = new Dictionary<int, List<int>>(); //所有房間之間的連結
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

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Collect Wall & Door Positions from Scene"))
        {
            CollectBarrierAndDoorPositions();
        }

        if (GUILayout.Button("Detect Rooms"))
        {
            DetectRooms();
        }

        GUILayout.Label($"Detected {detectedRooms.Count} room(s).", EditorStyles.boldLabel);

        EditorGUILayout.EndScrollView();
    }

    //取得所有barrier和door
    private void CollectBarrierAndDoorPositions()
    {
        //重置位置資料
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
            }
            else if (go.CompareTag("Door"))
            {
                doorPositions.Add(pos);
                barrierPositions.Add(pos);
            }
        }

        Debug.Log($"收集到 {barrierPositions.Count} 個邊界格子 (包含 {doorPositions.Count} 個門) (範圍 {detectBoundsStart} ~ {detectBoundsEnd})。\n");
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
    //偵測由門所連結的房間
    private void BuildDoorBasedRoomConnections()
    {
        // 重置房間連線資料
        roomConnections.Clear();

        // 建立一個字典：紀錄每一個格子所屬的房間索引
        Dictionary<Vector3Int, int> cellToRoom = new Dictionary<Vector3Int, int>();
        for (int i = 0; i < detectedRooms.Count; i++)
        {
            foreach (var cell in detectedRooms[i])
            {
                cellToRoom[cell] = i;
            }
        }

        // 遍歷所有門的位置
        foreach (var door in doorPositions)
        {
            // 儲存「此門鄰近」的所有房間編號
            HashSet<int> connectedRooms = new HashSet<int>();

            // 檢查門的四個方向
            foreach (var dir in GetFourDirections())
            {
                Vector3Int neighbor = door + dir;

                if (cellToRoom.TryGetValue(neighbor, out int roomIndex))
                {
                    connectedRooms.Add(roomIndex);
                }
            }

            // 把 HashSet 轉為陣列，方便兩兩配對
            int[] roomArray = new int[connectedRooms.Count];
            connectedRooms.CopyTo(roomArray);

            // 記錄雙向房間連結
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

            //將房間資訊指定給門 GameObject
            Vector3 worldPos = buildingGrid.GetCellCenterWorld(door);
            //Debug.Log("Try find door world position: " + worldPos);
            Collider[] hits = Physics.OverlapSphere(worldPos, 0.1f); // 找該位置附近的物件
            GameObject doorObject = null;

            foreach (var hit in hits)
            {
                //Debug.Log(hit);
                if (hit.CompareTag("Door"))
                {
                    doorObject = hit.gameObject;
                    break;
                }
                //Debug.Log("==============");
            }
            /*
            if (doorObject != null)
            {
                DoorRoomLink link = doorObject.GetComponent<DoorRoomLink>();
                if (link == null)
                    link = doorObject.AddComponent<DoorRoomLink>();

                link.connectedRoomIndices = roomArray.ToList();
            }
            */
        }
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
