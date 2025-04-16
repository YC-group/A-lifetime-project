using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelSaveAndLoadEditorWindow : EditorWindow
{
    private Grid buildingGrid;

    private Vector3Int gridSize = new Vector3Int(10, 3, 10); // 可編輯的 3D 地圖格子大小（包含 Y 軸）
    private int yMin = 0; // Y 軸最小值
    private int yMax = 0; // Y 軸最大值

    private Vector3 offset = new Vector3(0f, 0.5f, 0f); // 用於微調房間框框視覺化位置的偏移量

    private HashSet<Vector3Int> wallPositions = new HashSet<Vector3Int>(); // 所有牆壁的位置集合
    private List<HashSet<Vector3Int>> detectedRooms = new List<HashSet<Vector3Int>>(); // 偵測到的房間（封閉區域）

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

    // 在 Scene 中繪製偵測到的房間格子（以半透明的綠色框顯示）
    private void OnSceneGUI(SceneView sceneView)
    {
        if (detectedRooms == null) return;

        foreach (var room in detectedRooms)
        {
            foreach (var pos in room)
            {
                // 計算顯示位置，加入偏移量調整（可在 Inspector 調整）
                Vector3 worldPos = buildingGrid.CellToWorld(pos) + offset;

                // 繪製綠色透明框表示此為偵測到的房間格子
                Handles.color = new Color(0f, 1f, 0f, 0.25f);
                Handles.DrawWireCube(worldPos, buildingGrid.cellSize);

                // --- 以下為可選的地板視覺化效果 ---
                // 註解掉平面區塊顯示（若需要地面效果可取消註解）
                /*
                Handles.DrawSolidRectangleWithOutline(
                    new Vector3[] {
                        worldPos + new Vector3(-0.5f, -0.5f, -0.5f),
                        worldPos + new Vector3(0.5f, -0.5f, -0.5f),
                        worldPos + new Vector3(0.5f, -0.5f, 0.5f),
                        worldPos + new Vector3(-0.5f, -0.5f, 0.5f)
                    },
                    new Color(0f, 1f, 0f, 0.05f),
                    new Color(0f, 1f, 0f, 0.3f)
                );
                */
            }
        }
    }

    private void OnGUI()
    {
        // 設定關聯的 Grid
        buildingGrid = (Grid)EditorGUILayout.ObjectField("Building Grid", buildingGrid, typeof(Grid), true);

        // 設定地圖範圍與 Y 軸偵測上下界
        gridSize = EditorGUILayout.Vector3IntField("Grid Size", gridSize);
        yMin = EditorGUILayout.IntSlider("Y Min", yMin, 0, gridSize.y - 1);
        yMax = EditorGUILayout.IntSlider("Y Max", yMax, 0, gridSize.y - 1);
        yMin = Mathf.Min(yMin, yMax);

        // 顯示視覺化偏移量（可手動調整）
        offset = EditorGUILayout.Vector3Field("Visual Offset", offset);

        // 按鈕：從場景收集牆壁方塊
        if (GUILayout.Button("Collect Wall Positions from Scene"))
        {
            CollectWallPositions();
        }

        // 按鈕：開始進行房間偵測
        if (GUILayout.Button("Detect Rooms"))
        {
            DetectRooms();
        }

        // 顯示偵測到的房間數量
        GUILayout.Label($"Detected {detectedRooms.Count} room(s).", EditorStyles.boldLabel);
    }

    // 從場景中收集所有具有 Barrier 標籤的物件位置
    private void CollectWallPositions()
    {
        wallPositions.Clear();
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject go in allObjects)
        {
            if (go.CompareTag("Barrier"))
            {
                Vector3Int pos = buildingGrid.WorldToCell(go.transform.position);
                if (pos.y >= yMin && pos.y <= yMax)
                {
                    wallPositions.Add(pos);
                }
            }
        }
        Debug.Log($"收集到 {wallPositions.Count} 個屏障方塊 (Y 範圍 {yMin}~{yMax})。\n");
    }

    // 執行 Flood Fill 偵測所有被牆壁完全包圍的封閉空間
    private void DetectRooms()
    {
        detectedRooms.Clear();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    if (!wallPositions.Contains(pos) && !visited.Contains(pos))
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
    }

    // 使用 Flood Fill 找出所有相連的非牆壁格子區域
    private HashSet<Vector3Int> FloodFill(Vector3Int start, HashSet<Vector3Int> visited)
    {
        HashSet<Vector3Int> region = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            if (visited.Contains(current) || wallPositions.Contains(current)) continue;
            if (!IsInBounds(current)) continue;

            visited.Add(current);
            region.Add(current);

            // 加入四個方向相鄰的方塊
            foreach (Vector3Int dir in GetFourDirections())
            {
                Vector3Int next = current + dir;
                if (!visited.Contains(next) && !wallPositions.Contains(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return region;
    }

    // 檢查 Flood Fill 找出的區域是否為被 Barrier 完全圍住的封閉房間
    private bool IsRoomEnclosed(HashSet<Vector3Int> region)
    {
        foreach (var pos in region)
        {
            foreach (Vector3Int dir in GetFourDirections())
            {
                Vector3Int neighbor = pos + dir;
                if (!region.Contains(neighbor) && !wallPositions.Contains(neighbor))
                {
                    return false; // 有相鄰格為空氣且未被 Barrier 擋住，代表不封閉
                }
            }
        }
        return true;
    }

    // 限制位置是否在使用者指定的區域內（避免檢查到範圍外）
    private bool IsInBounds(Vector3Int pos)
    {
        return pos.x >= 0 && pos.y >= yMin && pos.z >= 0 &&
               pos.x < gridSize.x && pos.y <= yMax && pos.z < gridSize.z;
    }

    // 四個主要方向（不考慮 Y 軸）
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
