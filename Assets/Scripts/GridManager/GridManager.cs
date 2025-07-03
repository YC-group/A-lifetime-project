using UnityEngine;

/// <summary>
/// 網格系統 - Jerry0401
/// </summary>
///
public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    private GameObject[,] moveGridArray;
    
    private Grid moveGrid;

    private float cellSize = 3f;
    private int gridWidth = 10;
    private int gridHeight = 10;
    
    public static GridManager GetInstance() // Singleton
    {
        if (Instance == null)
        {
            Instance = GameObject.FindAnyObjectByType<GridManager>();
            if (Instance == null)
            {
                Debug.LogError("No GridManager found");
                return null;
            }
        }
        return Instance;
    }

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple GridManagers found");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        InitializedGrid();
    }
    
    private void OnDestroy()
    {
        // Singleton
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void InitializedGrid()
    {
        moveGrid = GameObject.FindWithTag("MoveGrid").GetComponent<Grid>();;
        moveGridArray = new GameObject[gridWidth, gridHeight];
    }

    public void AddGameObjectToMoveGrid(GameObject gameObject)
    {
        Vector3Int currentCell = moveGrid.WorldToCell(gameObject.transform.position);
        moveGridArray[currentCell.x, currentCell.z] = gameObject;
    }

    public void RemoveGameObjectFromMoveGrid(GameObject gameObject)
    {
        Vector3Int currentCell = moveGrid.WorldToCell(gameObject.transform.position);
        moveGridArray[currentCell.x, currentCell.z] = null;
    }

    public GameObject GetGameObjectFromMoveGrid(Vector3Int position)
    {
        return moveGridArray[position.x, position.z];
    }
    /// <summary>
    /// 更新移動網格
    /// </summary>
    /// <returns>void</returns>
    public void UpdateGameObjectFromMoveGrid(GameObject gameObject, Vector3Int nextCell)
    {
        Vector3Int currentCell = moveGrid.WorldToCell(gameObject.transform.position);
        moveGridArray[currentCell.x, currentCell.z] = null;
        moveGridArray[nextCell.x, nextCell.z] = gameObject;
    }
    /// <summary>
    /// 查詢網格是否已被占用
    /// </summary>
    /// <returns>void</returns>
    public bool IsOccupied(Vector3Int position)
    {
        if (moveGridArray[position.x, position.z] != null)
        {
            return true;
        }
        return false;
    }
}
