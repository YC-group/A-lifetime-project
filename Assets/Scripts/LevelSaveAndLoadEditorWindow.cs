using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 關卡存讀檔流程 - js5515
/// </summary>

public class LevelSaveAndLoadEditorWindow : EditorWindow
{
    #region 變數
    private Grid buildingGrid;

    private Vector3Int detectBoundsStart = Vector3Int.zero;
    private Vector3Int detectBoundsEnd = new Vector3Int(10, 4, 10);
    private Vector3 gridOffset = new Vector3(0f, 0.5f, 0f);

    private Color roomColor = new Color(0f, 1f, 0f, 0.25f);
    private Color detectBoundsColor = new Color(1f, 0f, 0f, 1f);
    private Color roomConnectionLineColor = new Color(0.5f, 0f, 1f, 1f);
    private float roomConnectionLineThickness = 10f;

    private HashSet<Vector3Int> barrierPositions = new HashSet<Vector3Int>();
    private HashSet<Vector3Int> doorPositions = new HashSet<Vector3Int>();
    private List<HashSet<Vector3Int>> detectedRooms = new List<HashSet<Vector3Int>>();
    private Dictionary<int, List<int>> roomConnections = new Dictionary<int, List<int>>();
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

        foreach (var room in detectedRooms)
        {
            foreach (var pos in room)
            {
                Vector3 worldPos = buildingGrid.GetCellCenterWorld(pos) + Vector3.Scale(gridOffset, buildingGrid.cellSize);
                Handles.color = roomColor;
                Handles.CubeHandleCap(0, worldPos, Quaternion.identity, Mathf.Max(buildingGrid.cellSize.x, buildingGrid.cellSize.z), EventType.Repaint);
            }
        }

        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        Handles.color = detectBoundsColor;
        Vector3 min = buildingGrid.CellToWorld(detectBoundsStart) + Vector3.Scale(gridOffset, buildingGrid.cellSize);
        Vector3 max = buildingGrid.CellToWorld(detectBoundsEnd) + Vector3.Scale(gridOffset, buildingGrid.cellSize);
        Vector3 size = max - min + buildingGrid.cellSize;
        Handles.DrawWireCube(min + size / 2f, size);
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

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
        buildingGrid = (Grid)EditorGUILayout.ObjectField("Building Grid", buildingGrid, typeof(Grid), true);

        detectBoundsStart = EditorGUILayout.Vector3IntField("Grid Start", detectBoundsStart);
        detectBoundsEnd = EditorGUILayout.Vector3IntField("Grid End", detectBoundsEnd);
        gridOffset = EditorGUILayout.Vector3Field("Visual Offset", gridOffset);

        roomColor = EditorGUILayout.ColorField("Room Color", roomColor);
        detectBoundsColor = EditorGUILayout.ColorField("Bounds Color", detectBoundsColor);
        roomConnectionLineColor = EditorGUILayout.ColorField("Connection Line Color", roomConnectionLineColor);
        roomConnectionLineThickness = EditorGUILayout.Slider("Connection Line Thickness", roomConnectionLineThickness, 0.1f, 10f);

        if (GUILayout.Button("Collect Wall & Door Positions from Scene"))
        {
            CollectWallAndDoorPositions();
        }

        if (GUILayout.Button("Detect Rooms"))
        {
            DetectRooms();
        }

        GUILayout.Label($"Detected {detectedRooms.Count} room(s).", EditorStyles.boldLabel);
    }

    private void CollectWallAndDoorPositions()
    {
        barrierPositions.Clear();
        doorPositions.Clear();
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject go in allObjects)
        {
            Vector3Int pos = buildingGrid.WorldToCell(go.transform.position);
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

    private void DetectRooms()
    {
        detectedRooms.Clear();
        roomConnections.Clear();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        for (int x = detectBoundsStart.x; x <= detectBoundsEnd.x; x++)
        {
            for (int y = detectBoundsStart.y; y <= detectBoundsEnd.y; y++)
            {
                for (int z = detectBoundsStart.z; z <= detectBoundsEnd.z; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);

                    if (!visited.Contains(pos) && !barrierPositions.Contains(pos))
                    {
                        var room = FloodFill(pos, visited);
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

        foreach (var door in doorPositions)
        {
            HashSet<int> connectedRooms = new HashSet<int>();
            foreach (var dir in GetFourDirections())
            {
                Vector3Int neighbor = door + dir;
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
                    if (!roomConnections.ContainsKey(roomArray[i])) roomConnections[roomArray[i]] = new List<int>();
                    if (!roomConnections[roomArray[i]].Contains(roomArray[j])) roomConnections[roomArray[i]].Add(roomArray[j]);

                    if (!roomConnections.ContainsKey(roomArray[j])) roomConnections[roomArray[j]] = new List<int>();
                    if (!roomConnections[roomArray[j]].Contains(roomArray[i])) roomConnections[roomArray[j]].Add(roomArray[i]);
                }
            }
        }
    }

    private Vector3 GetRoomCenter(HashSet<Vector3Int> room)
    {
        Vector3 sum = Vector3.zero;
        foreach (var cell in room)
        {
            sum += buildingGrid.GetCellCenterWorld(cell) + Vector3.Scale(gridOffset, buildingGrid.cellSize);
        }
        return sum / room.Count;
    }

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

    private bool IsInBounds(Vector3Int pos)
    {
        return pos.x >= detectBoundsStart.x && pos.x <= detectBoundsEnd.x &&
               pos.y >= detectBoundsStart.y && pos.y <= detectBoundsEnd.y &&
               pos.z >= detectBoundsStart.z && pos.z <= detectBoundsEnd.z;
    }

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
