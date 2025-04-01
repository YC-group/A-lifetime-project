using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;
/// <summary>
/// ���d�s�边 - js5515
/// </summary>
[ExecuteAlways]
public class LevelEditorWindow : EditorWindow
{
    #region �ܼ�
    private Vector2 scrollPos;

    private int selectedToolTab = 0;
    private string[] toolTabNames = { "�]�w", "�ت�", "����", "�D��" };

    private int selectedGridAlignModeTab = 0;
    private string[] gridAlignModeTabNames = { "�L���", "����ؿv����", "������ʺ���"};

    private GameObject level;

    private Grid buildingGrid;
    private Color buildingGridColor = Color.blue;
    private int buildingGridSize = 10;

    private Grid moveGrid;
    private Color moveGridColor = Color.yellow;
    private int moveGridSize = 10;

    private string savePath = "Assets/Scenes/Levels"; // �w�]�x�s���|
    private string sceneName = "NewScene"; // �w�]�����W��
    #endregion

    [MenuItem("Tools/Level Editor Window")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnEnable()
    {
        this.minSize = new Vector2(400, 300); // �̤p�ؤo (�e, ��)
        //maxSize = new Vector2(800, 600); // �̤j�ؤo (�e, ��)

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

    private static GameObject selectedObject; // ��e�Q���������
    private static bool isDragging = false;   // �O�_���b�즲
    private static Vector3Int lastGridPosition;
    private void AlignObjectByMouse(Grid grid)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (!isDragging)
            {
                // ���տ������
                TrySelectObject();
            }
            else
            {
                // ��U����
                isDragging = false;
                selectedObject = null;
                SceneView.RepaintAll();
            }

            e.Use();
        }

        if (isDragging && selectedObject != null)
        {
            // ��������H�ƹ��öK������
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

            // �ץ��������A�Ϫ������l���ӫD���I�W
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

        EditorGUILayout.LabelField("����Ҧ�", GUILayout.Width(50));
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

        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual; // �}�Ҳ`�״���

        Handles.color = color;
        Vector3 origin = grid.transform.position;
        Vector3 cellSize = grid.cellSize;

        // �p�ⰾ���q�A�����󪺤��߹�Ǻ��椤��
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

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always; // ��_���L���`��
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
