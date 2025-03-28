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
    private string[] tabNames = { "�]�w", "�ت�", "����", "�D��" };

    private GameObject level;

    private Grid buildingGrid;
    private Color buildingGridColor = Color.blue;
    private int buildingGridSize = 10;

    private Grid moveGrid;
    private Color moveGridColor = Color.yellow;
    private int moveGridSize = 10;

    private string savePath = "Assets/Scenes/Levels"; // �w�]�x�s���|
    private string sceneName = "NewScene"; // �w�]�����W��

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

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // ���j�u

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
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // ���j�u

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
            Debug.LogError($"���w�����|���s�b: {savePath}");
            return;
        }

        string originalScenePath = EditorSceneManager.GetActiveScene().path;
        if (string.IsNullOrEmpty(originalScenePath))
        {
            Debug.LogError("��e�����|���x�s�L�A�Х�����x�s�����I");
            return;
        }

        string newScenePath = $"{savePath}/{sceneName}.unity";

        // �T�O�ɦW������
        int count = 1;
        while (File.Exists(newScenePath))
        {
            newScenePath = $"{savePath}/{sceneName}_{count}.unity";
            count++;
        }

        // �t�s������������
        bool success = EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), newScenePath, false);

        if (success)
        {
            Debug.Log($"�������\�t�s��: {newScenePath}�A�����O���b�쥻���s������I");
        }
        else
        {
            Debug.LogError("�x�s�����ɵo�Ϳ��~�I");
        }
    }
}
