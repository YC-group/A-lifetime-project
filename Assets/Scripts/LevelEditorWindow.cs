using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
/// <summary>
/// ���d�s�边 - js5515
/// </summary>
[ExecuteAlways]
public class LevelEditorWindow : EditorWindow
{
    #region �ܼ�
    private Vector2 scrollPos; //�u�ʵ����Ѽ�

    private int selectedToolTab = 0; //��ܤu��tab���Ѽ�
    private string[] toolTabNames = { "�]�w", "�ت�", "����", "�D��" }; //��ܤu��tab���ﶵ

    private int selectedGridAlignModeTab = 0; //��ܹ���Ҧ����Ѽ�
    private string[] gridAlignModeTabNames = { "�L���", "����ؿv����", "������ʺ���"}; //��ܹ���Ҧ����ﶵ

    //����Ҧ��ΰѼ�
    private static GameObject selectedObject; // ��e�Q���������
    private static bool isDragging = false;   // �O�_���b�즲
    private static Vector3Int lastGridPosition;

    private GameObject level;
    private Vector3 gridOffset = new Vector3(0, 0.5f, 0); //���ܺ����ı�W��ܪ���m(���|�v�T�u��������)

    //�ؿv�κ���Ѽ�
    private Grid buildingGrid;
    private Color buildingGridColor = Color.blue;
    private int buildingGridSize = 31;

    //���ʥκ���Ѽ�
    private Grid moveGrid;
    private Color moveGridColor = Color.yellow;
    private int moveGridSize = 10;

    /*
    private string savePath = "Assets/Scenes/Levels"; // �w�]�x�s���|
    private string sceneName = "NewScene"; // �w�]�����W��
    */
    #endregion

    [MenuItem("Tools/Level Editor Window")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnEnable()
    {
        this.minSize = new Vector2(400, 300); // �]�w�̤p�ؤo (�e, ��)
        //maxSize = new Vector2(800, 600); // �̤j�ؤo (�e, ��)

        SceneView.duringSceneGui += OnSceneGUI;

        //�۰ʴM�䪫��
        if (level == null) level = GameObject.FindWithTag("Level");
        if (buildingGrid == null) buildingGrid = GameObject.FindWithTag("BuildingGrid").GetComponent<Grid>();
        if (moveGrid == null) moveGrid = GameObject.FindWithTag("MoveGrid").GetComponent<Grid>();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    //����եΡA����Update
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


    // �즲����ù�������W���޿�
    private void AlignObjectByMouse(Grid grid)
    {
        Event e = Event.current;

        // ��ƹ�������U��
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (!isDragging)
            {
                // �p�G�ثe�S�����b�즲 �� ���տ������
                TrySelectObject();
            }
            else
            {
                // �p�G�w�b�즲�� �� ��U����A�������
                isDragging = false;
                selectedObject = null;
                SceneView.RepaintAll(); // ��s Scene ����
            }

            e.Use(); // �i�� Unity �o�Өƥ�w�B�z�A�קK�ǻ����L�u��
        }

        // �Y���b�즲�B����F���� �� �������۷ƹ����ʨù������
        if (isDragging && selectedObject != null)
        {
            MoveObjectWithMouse(grid);
        }
    }

    // ���տ���ƹ��U������
    private bool TrySelectObject()
    {
        // �N GUI �y���ഫ���@�ɤ��� Ray
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;

        // �g�u�˴��G�p�G�����F�� Collider ������
        if (Physics.Raycast(ray, out hit))
        {
            selectedObject = hit.collider.gameObject; // ����Ӫ���
            isDragging = true;                        // �}�l�즲�Ҧ�
            return true;
        }

        return false; // �S����쪫��
    }

    // �������������H�ƹ��ù�������
    private void MoveObjectWithMouse(Grid grid)
    {
        // �A�����o�ƹ��g�u
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        // �إߤ@�Ӥ��������������]�Ω�p��ƹ��P�a�����I�^
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        // �p��g�u�P�������I
        if (plane.Raycast(ray, out float enter))
        {
            // ���o���I�@�ɮy��
            Vector3 worldPosition = ray.GetPoint(enter);

            // �O�d��l Y �ȡA�קK����ɰ��έ��C
            worldPosition.y = selectedObject.transform.position.y;

            // �N�@�ɮy���ഫ�� Grid �� Cell �y��
            Vector3Int gridPosition = grid.WorldToCell(worldPosition);

            // ������l������
            Vector3 alignedPosition = grid.GetCellCenterWorld(gridPosition);

            // �p�G��m���ܧ�A�N��s�����m
            if (gridPosition != lastGridPosition)
            {
                selectedObject.transform.position = alignedPosition;
                lastGridPosition = gridPosition;
                SceneView.RepaintAll(); // ���sø�s����
            }
        }
    }

    //�N����������
    private void AlignToGrid(Grid grid, GameObject go)
    {
        go.transform.position = grid.GetCellCenterWorld(grid.WorldToCell(go.transform.position));
    }

    //EditorWindow����ܤ��e
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

    //�]�wTab�����e
    private void SettingsTabGUI()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("����Ҧ�", GUILayout.Width(50));
        selectedGridAlignModeTab = GUILayout.Toolbar(selectedGridAlignModeTab, gridAlignModeTabNames);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("�N����������ؿv����"))
        {
            foreach (var go in Selection.gameObjects)
            {
                AlignToGrid(buildingGrid, go);
            }
        }
        if (GUILayout.Button("�N������������ʺ���"))
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

    //�����ı���U
    private void DrawGrid(Grid grid, Color color, int gridSize, Vector3 offset)
    {
        if (grid == null) return;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual; // �}�Ҳ`�״���

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

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always; // ��_���L���`��
    }

    /*
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
    */
}
