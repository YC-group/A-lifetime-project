using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class LevelEditor : MonoBehaviour
{
    public Grid buildingGrid;
    public Color buildingGridColor = Color.blue;
    public int buildingGridSize = 10; // 自訂 Building Grid 的大小

    public Grid moveGrid;
    public Color moveGridColor = Color.yellow;
    public int moveGridSize = 10; // 自訂 Move Grid 的大小

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (buildingGrid) DrawGrid(buildingGrid, buildingGridColor, buildingGridSize);
        if (moveGrid) DrawGrid(moveGrid, moveGridColor, moveGridSize);
        SceneView.RepaintAll();
    }

    private void DrawGrid(Grid grid, Color color, int gridSize)
    {
        Handles.color = color;
        Vector3 origin = grid.transform.position;
        Vector3 cellSize = grid.cellSize;

        // 繪製 X 軸線
        for (int x = 0; x <= gridSize; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize.x, 0, 0);
            Vector3 end = start + new Vector3(0, 0, gridSize * cellSize.z);
            Handles.DrawLine(start, end);
        }

        // 繪製 Z 軸線
        for (int z = 0; z <= gridSize; z++)
        {
            Vector3 start = origin + new Vector3(0, 0, z * cellSize.z);
            Vector3 end = start + new Vector3(gridSize * cellSize.x, 0, 0);
            Handles.DrawLine(start, end);
        }
    }
}
