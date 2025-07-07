using UnityEngine;

/// <summary>
/// 網格系統 - Jerry0401
/// </summary>
///
public class GridManager : MonoBehaviour
{
    private static GridManager Instance;
    
    public Grid moveGrid; // 移動網格
    
    private GameObject[,] moveGridArray; // 網格陣列，用於儲存網格上的內容
    
    [Header("網格設定")]
    [SerializeField] private float cellSize = 3f;
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    
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
    
    public GameObject GetGameObjectFromMoveGrid(Vector3 position)
    {
        Vector3Int cell = moveGrid.WorldToCell(position);
        return moveGridArray[cell.x, cell.z];
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
    /// 更新移動網格
    /// </summary>
    /// <returns>void</returns>
    public void UpdateGameObjectFromMoveGrid(GameObject gameObject, Vector3 position)
    {
        Vector3Int currentCell = moveGrid.WorldToCell(gameObject.transform.position);
        Vector3Int nextCell = moveGrid.WorldToCell(position);
        moveGridArray[currentCell.x, currentCell.z] = null;
        moveGridArray[nextCell.x, nextCell.z] = gameObject;
    }
    /// <summary>
    /// 查詢網格是否已被敵人占用
    /// </summary>
    /// <returns>void</returns>
    public bool IsOccupiedByEnemy(Vector3Int position)
    {
        if (moveGridArray[position.x, position.z] != null && moveGridArray[position.x, position.z].CompareTag("Enemy"))
        {
            return true;
        }
        return false;
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
    /// <summary>
    /// 查詢網格是否被指定物件占用
    /// </summary>
    /// <returns>void</returns>
    public bool IsOccupiedByObjectWithTag(Vector3Int position, string tag)
    {
        if (moveGridArray[position.x, position.z] != null && moveGridArray[position.x, position.z].CompareTag(tag))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// 查詢網格是否被占用且忽略指定物件
    /// </summary>
    /// <returns>void</returns>
    public bool IsOccupiedByObjectWithoutTag(Vector3Int position, string tag)
    {
        if (moveGridArray[position.x, position.z] != null && !moveGridArray[position.x, position.z].CompareTag(tag))
        {
            return true;
        }
        return false;
    }
}
