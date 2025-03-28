using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;

[ExecuteAlways]
public class LevelEditorWindow : EditorWindow
{
    private Vector2 scrollPos;

    private int selectedTab = 0;
    private string[] tabNames = { "設定", "建物", "角色", "道具" };

    private GameObject level;

    private Grid buildingGrid;
    private Color buildingGridColor = Color.blue;
    private int buildingGridSize = 10;

    private Grid moveGrid;
    private Color moveGridColor = Color.yellow;
    private int moveGridSize = 10;

    private string savePath = "Assets/Scenes/Levels"; // 預設儲存路徑
    private string sceneName = "NewScene"; // 預設場景名稱

    [MenuItem("Tools/Level Editor Window")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        if (level == null) level = GameObject.FindWithTag("Level");
        if (buildingGrid == null) buildingGrid = GameObject.FindWithTag("BuildingGrid").GetComponent<Grid>();
        if (moveGrid == null) moveGrid = GameObject.FindWithTag("MoveGrid").GetComponent<Grid>();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

        switch (selectedTab)
        {
            case 0:
                SettingsGUI();
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
        }
    }

    private void SettingsGUI()
    {
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

        if (GUILayout.Button("Refresh Scene View"))
        {
            SceneView.RepaintAll();
        }

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
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (buildingGrid) DrawGrid(buildingGrid, buildingGridColor, buildingGridSize);
        if (moveGrid) DrawGrid(moveGrid, moveGridColor, moveGridSize);
    }

    private void DrawGrid(Grid grid, Color color, int gridSize)
    {
        if (grid == null) return;

        Handles.color = color;
        Vector3 origin = grid.transform.position;
        Vector3 cellSize = grid.cellSize;

        for (int x = 0; x <= gridSize; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize.x, 0, 0);
            Vector3 end = start + new Vector3(0, 0, gridSize * cellSize.z);
            Handles.DrawLine(start, end);
        }

        for (int z = 0; z <= gridSize; z++)
        {
            Vector3 start = origin + new Vector3(0, 0, z * cellSize.z);
            Vector3 end = start + new Vector3(gridSize * cellSize.x, 0, 0);
            Handles.DrawLine(start, end);
        }
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
