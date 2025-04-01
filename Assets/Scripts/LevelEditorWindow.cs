using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;
/// <summary>
/// 關卡編輯器 - js5515
/// </summary>
[ExecuteAlways]
public class LevelEditorWindow : EditorWindow
{
    #region 變數
    private Vector2 scrollPos;

    private int selectedToolTab = 0;
    private string[] toolTabNames = { "設定", "建物", "角色", "道具" };

    private int selectedGridAlignModeTab = 0;
    private string[] gridAlignModeTabNames = { "無對齊", "對齊建築網格", "對齊移動網格"};

    private GameObject level;

    private Grid buildingGrid;
    private Color buildingGridColor = Color.blue;
    private int buildingGridSize = 10;

    private Grid moveGrid;
    private Color moveGridColor = Color.yellow;
    private int moveGridSize = 10;

    private string savePath = "Assets/Scenes/Levels"; // 預設儲存路徑
    private string sceneName = "NewScene"; // 預設場景名稱
    #endregion

    [MenuItem("Tools/Level Editor Window")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnEnable()
    {
        this.minSize = new Vector2(400, 300); // 最小尺寸 (寬, 高)
        //maxSize = new Vector2(800, 600); // 最大尺寸 (寬, 高)

        SceneView.duringSceneGui += OnSceneGUI;
        if (level == null) level = GameObject.FindWithTag("Level");
        if (buildingGrid == null) buildingGrid = GameObject.FindWithTag("BuildingGrid").GetComponent<Grid>();
        if (moveGrid == null) moveGrid = GameObject.FindWithTag("MoveGrid").GetComponent<Grid>();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (buildingGrid) DrawGrid(buildingGrid, buildingGridColor, buildingGridSize);
        if (moveGrid) DrawGrid(moveGrid, moveGridColor, moveGridSize);

        switch (selectedGridAlignModeTab)
        {
            case 0:
                break;
            case 1:
                Selection.activeGameObject = null;
                AlignObjectByMouse(buildingGrid);
                break;
            case 2:
                Selection.activeGameObject = null;
                AlignObjectByMouse(moveGrid);
                break;
        }
    }

    private static GameObject selectedObject; // 當前被選取的物件
    private static bool isDragging = false;   // 是否正在拖曳
    private static Vector3Int lastGridPosition;
    private void AlignObjectByMouse(Grid grid)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (!isDragging)
            {
                // 嘗試選取物件
                TrySelectObject();
            }
            else
            {
                // 放下物件
                isDragging = false;
                selectedObject = null;
                SceneView.RepaintAll();
            }

            e.Use();
        }

        if (isDragging && selectedObject != null)
        {
            // 讓物件跟隨滑鼠並貼齊網格
            MoveObjectWithMouse(grid);
        }
    }
    private bool TrySelectObject()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            selectedObject = hit.collider.gameObject;
            isDragging = true;
            return true;
        }

        return false;
    }


    private void MoveObjectWithMouse(Grid grid)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 worldPosition = ray.GetPoint(enter);
            worldPosition.y = selectedObject.transform.position.y;
            Vector3Int gridPosition = grid.WorldToCell(worldPosition);

            // 修正對齊網格，使物件位於格子內而非交點上
            Vector3 alignedPosition = grid.CellToWorld(gridPosition);

            if (gridPosition != lastGridPosition)
            {
                selectedObject.transform.position = alignedPosition;
                lastGridPosition = gridPosition;
                SceneView.RepaintAll();
            }
        }
    }
    private void AlignToGrid(Grid grid, GameObject go)
    {
        go.transform.position = grid.CellToWorld(grid.WorldToCell(go.transform.position));
    }

    private void OnGUI()
    {
        selectedToolTab = GUILayout.Toolbar(selectedToolTab, toolTabNames);

        switch (selectedToolTab)
        {
            case 0:
                SettingsTabGUI();
                break;
            case 1:
                BuildingTabGUI();
                break;
            case 2:
                break;
            case 3:
                break;
        }
    }    

    private void SettingsTabGUI()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("對齊模式", GUILayout.Width(50));
        selectedGridAlignModeTab = GUILayout.Toolbar(selectedGridAlignModeTab, gridAlignModeTabNames);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Align to building grid"))
        {
            foreach (var go in Selection.gameObjects)
            {
                AlignToGrid(buildingGrid, go);
            }
        }
        if (GUILayout.Button("Align to move grid"))
        {
            foreach (var go in Selection.gameObjects)
            {
                AlignToGrid(moveGrid, go);
            }
        }

        EditorGUILayout.EndHorizontal();


        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));

        GUILayout.Label("Level Object", EditorStyles.boldLabel);
        level = (GameObject)EditorGUILayout.ObjectField("Level Object", level, typeof(GameObject), true);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // 分隔線

        GUILayout.Label("Building Grid Settings", EditorStyles.boldLabel);
        buildingGrid = (Grid)EditorGUILayout.ObjectField("Building Grid", buildingGrid, typeof(Grid), true);
        buildingGridColor = EditorGUILayout.ColorField("Building Grid Color", buildingGridColor);
        buildingGridSize = EditorGUILayout.IntSlider("Building Grid Size", buildingGridSize, 1, 50);

        GUILayout.Space(10);

        GUILayout.Label("Move Grid Settings", EditorStyles.boldLabel);
        moveGrid = (Grid)EditorGUILayout.ObjectField("Move Grid", moveGrid, typeof(Grid), true);
        moveGridColor = EditorGUILayout.ColorField("Move Grid Color", moveGridColor);
        moveGridSize = EditorGUILayout.IntSlider("Move Grid Size", moveGridSize, 1, 50);

        GUILayout.Space(10);


        /*
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // 分隔線

        GUILayout.Label("Save Scene As...", EditorStyles.boldLabel);
        savePath = EditorGUILayout.TextField("Save Path", savePath);
        sceneName = EditorGUILayout.TextField("New Scene Name", sceneName);

        if (GUILayout.Button("Save Current Scene As..."))
        {
            SaveCurrentSceneAs();
        }
        */

        EditorGUILayout.EndScrollView();

        SceneView.RepaintAll();
    }

    private void BuildingTabGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));



        EditorGUILayout.EndScrollView();
    }


    private void DrawGrid(Grid grid, Color color, int gridSize)
    {
        if (grid == null) return;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual; // 開啟深度測試

        Handles.color = color;
        Vector3 origin = grid.transform.position;
        Vector3 cellSize = grid.cellSize;

        // 計算偏移量，讓物件的中心對準網格中心
        Vector3 offset = new Vector3(cellSize.x * 0.5f, 0, cellSize.z * 0.5f);

        for (int x = 0; x <= gridSize; x++)
        {
            Vector3 start = origin + offset + new Vector3(x * cellSize.x, 0, 0);
            Vector3 end = start + new Vector3(0, 0, gridSize * cellSize.z);
            Handles.DrawLine(start, end);
        }

        for (int z = 0; z <= gridSize; z++)
        {
            Vector3 start = origin + offset + new Vector3(0, 0, z * cellSize.z);
            Vector3 end = start + new Vector3(gridSize * cellSize.x, 0, 0);
            Handles.DrawLine(start, end);
        }

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always; // 恢復為無視深度
    }


    private void SaveCurrentSceneAs()
    {
        if (!Directory.Exists(savePath))
        {
            Debug.LogError($"指定的路徑不存在: {savePath}");
            return;
        }

        string originalScenePath = EditorSceneManager.GetActiveScene().path;
        if (string.IsNullOrEmpty(originalScenePath))
        {
            Debug.LogError("當前場景尚未儲存過，請先手動儲存場景！");
            return;
        }

        string newScenePath = $"{savePath}/{sceneName}.unity";

        // 確保檔名不重複
        int count = 1;
        while (File.Exists(newScenePath))
        {
            newScenePath = $"{savePath}/{sceneName}_{count}.unity";
            count++;
        }

        // 另存場景但不切換
        bool success = EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), newScenePath, false);

        if (success)
        {
            Debug.Log($"場景成功另存為: {newScenePath}，但仍保持在原本的編輯場景！");
        }
        else
        {
            Debug.LogError("儲存場景時發生錯誤！");
        }
    }
}
