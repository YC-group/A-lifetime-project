using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
/// <summary>
/// 關卡編輯器 - js5515
/// </summary>
[ExecuteAlways]
public class LevelEditorWindow : EditorWindow
{
    #region 變數
    private Vector2 scrollPos; //滾動視窗參數

    private int selectedToolTab = 0; //選擇工具tab的參數
    private string[] toolTabNames = { "設定", "建物", "角色", "道具" }; //選擇工具tab的選項

    private int selectedGridAlignModeTab = 0; //選擇對齊模式的參數
    private string[] gridAlignModeTabNames = { "無對齊", "對齊建築網格", "對齊移動網格"}; //選擇對齊模式的選項

    //對齊模式用參數
    private static GameObject selectedObject; // 當前被選取的物件
    private static bool isDragging = false;   // 是否正在拖曳
    private static Vector3Int lastGridPosition;

    private GameObject level;
    private Vector3 gridOffset = new Vector3(0, 0.5f, 0); //改變網格視覺上顯示的位置(不會影響真正的網格)

    //建築用網格參數
    private Grid buildingGrid;
    private Color buildingGridColor = Color.blue;
    private int buildingGridSize = 31;

    //移動用網格參數
    private Grid moveGrid;
    private Color moveGridColor = Color.yellow;
    private int moveGridSize = 10;

    /*
    private string savePath = "Assets/Scenes/Levels"; // 預設儲存路徑
    private string sceneName = "NewScene"; // 預設場景名稱
    */
    #endregion

    [MenuItem("Tools/Level Editor Window")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnEnable()
    {
        this.minSize = new Vector2(400, 300); // 設定最小尺寸 (寬, 高)
        //maxSize = new Vector2(800, 600); // 最大尺寸 (寬, 高)

        SceneView.duringSceneGui += OnSceneGUI;

        //自動尋找物件
        if (level == null) level = GameObject.FindWithTag("Level");
        if (buildingGrid == null) buildingGrid = GameObject.FindWithTag("BuildingGrid").GetComponent<Grid>();
        if (moveGrid == null) moveGrid = GameObject.FindWithTag("MoveGrid").GetComponent<Grid>();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    //持續調用，類似Update
    private void OnSceneGUI(SceneView sceneView)
    {
        if (buildingGrid) DrawGrid(buildingGrid, buildingGridColor, buildingGridSize, gridOffset);
        if (moveGrid) DrawGrid(moveGrid, moveGridColor, moveGridSize, gridOffset);

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


    // 拖曳物件並對齊到網格上的邏輯
    private void AlignObjectByMouse(Grid grid)
    {
        Event e = Event.current;

        // 當滑鼠左鍵按下時
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (!isDragging)
            {
                // 如果目前沒有正在拖曳 → 嘗試選取物件
                TrySelectObject();
            }
            else
            {
                // 如果已在拖曳中 → 放下物件，取消選取
                isDragging = false;
                selectedObject = null;
                SceneView.RepaintAll(); // 更新 Scene 視圖
            }

            e.Use(); // 告知 Unity 這個事件已處理，避免傳遞到其他工具
        }

        // 若正在拖曳且選取了物件 → 讓物件跟著滑鼠移動並對齊網格
        if (isDragging && selectedObject != null)
        {
            MoveObjectWithMouse(grid);
        }
    }

    // 嘗試選取滑鼠下的物件
    private bool TrySelectObject()
    {
        // 將 GUI 座標轉換為世界中的 Ray
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;

        // 射線檢測：如果擊中了有 Collider 的物件
        if (Physics.Raycast(ray, out hit))
        {
            selectedObject = hit.collider.gameObject; // 選取該物件
            isDragging = true;                        // 開始拖曳模式
            return true;
        }

        return false; // 沒有選到物件
    }

    // 讓選取的物件跟隨滑鼠並對齊到網格
    private void MoveObjectWithMouse(Grid grid)
    {
        // 再次取得滑鼠射線
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        // 建立一個水平的虛擬平面（用於計算滑鼠與地面交點）
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        // 計算射線與平面交點
        if (plane.Raycast(ray, out float enter))
        {
            // 取得交點世界座標
            Vector3 worldPosition = ray.GetPoint(enter);

            // 保留原始 Y 值，避免物件升高或降低
            worldPosition.y = selectedObject.transform.position.y;

            // 將世界座標轉換為 Grid 的 Cell 座標
            Vector3Int gridPosition = grid.WorldToCell(worldPosition);

            // 對齊到格子的中心
            Vector3 alignedPosition = grid.GetCellCenterWorld(gridPosition);

            // 如果位置有變更，就更新物件位置
            if (gridPosition != lastGridPosition)
            {
                selectedObject.transform.position = alignedPosition;
                lastGridPosition = gridPosition;
                SceneView.RepaintAll(); // 重新繪製場景
            }
        }
    }

    //將物件對齊網格
    private void AlignToGrid(Grid grid, GameObject go)
    {
        go.transform.position = grid.GetCellCenterWorld(grid.WorldToCell(go.transform.position));
    }

    //EditorWindow的顯示內容
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

    //設定Tab的內容
    private void SettingsTabGUI()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("對齊模式", GUILayout.Width(50));
        selectedGridAlignModeTab = GUILayout.Toolbar(selectedGridAlignModeTab, gridAlignModeTabNames);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("將選取物件對齊建築網格"))
        {
            foreach (var go in Selection.gameObjects)
            {
                AlignToGrid(buildingGrid, go);
            }
        }
        if (GUILayout.Button("將選取物件對齊移動網格"))
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

        gridOffset = EditorGUILayout.Vector3Field("Grid offset", gridOffset);

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

    //網格視覺輔助
    private void DrawGrid(Grid grid, Color color, int gridSize, Vector3 offset)
    {
        if (grid == null) return;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual; // 開啟深度測試

        Handles.color = color;
        Vector3 origin = grid.transform.position;
        Vector3 cellSize = grid.cellSize;

        offset = Vector3.Scale(offset, moveGrid.cellSize);

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

    /*
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
    */
}
