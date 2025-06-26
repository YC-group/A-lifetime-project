using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敵人行動廣播 - Jerry0401
/// </summary>
///
public class EnemyActionScheduler : MonoBehaviour
{
    public GameObject[] EnemyGameObjects;
    private RoundState currentRound;
    private bool isMoving;

    void Start()
    {
        EnemyGameObjects = GameObject.FindGameObjectsWithTag("Enemy"); // 獲取所有敵人物件
        isMoving = false;
        foreach (var enemy in EnemyGameObjects)
        {
            GridManager.Instance.AddGameObjectToMoveGrid(enemy); // 將敵人物件存入網格陣列
        }
    }

    void Update()
    {
        currentRound = GameManager.Instance.GetCurrentRound(); // 確定現在回合
    }

    void FixedUpdate()
    {
        if (RoundState.EnemyTurn == currentRound)
        {
            // GameObject.FindWithTag("GameManager").GetComponent<GameManager>().SetToNextRound();
            if (!isMoving) // 是否正在移動
            {
                EnemyMoveSchedule(); // 廣播敵人行動
            }
            if (CheckEnemyArrived()) // 確定敵人動作結束
            {
                GameManager.Instance.SetToNextRound();
            }
        }
    }

    // 確定敵人動作結束
    private bool CheckEnemyArrived()
    {
        int actionComplete = 0; // 計算場景中動作結束的敵人數量
        foreach (var enemy in EnemyGameObjects)
        {
            if (enemy.GetComponent<EnemyScript>().targetPosition == enemy.transform.position)
            { 
                actionComplete++;
            }
        }
        if (actionComplete == EnemyGameObjects.Length)
        {
            // Debug.Log("Action Complete");
            isMoving = false;
            return true;
        }
        return false;
    }

    // 敵人移動規劃
    public void EnemyMoveSchedule()
    {
        isMoving = true;
        EnemyGameObjects = GameObject.FindGameObjectsWithTag("Enemy");
        if (EnemyGameObjects == null)
        {
            Debug.LogWarning("Can't found enemy !");
            return;
        }

        GameObject[] sortGameObjects =
            EnemyGameObjects.OrderByDescending(go => go.GetComponent<EnemyScript>().movePriority).ToArray(); // 將敵人按照 MovePriority 降冪排序
        foreach (var enemy in sortGameObjects)
        {
            if (enemy.GetComponent<EnemyScript>().isStun == false) // 在沒有被擊暈的狀態下
            {
                EnemyMovePathFinding(enemy); // 規劃移動路線
            }
        }
    }
    
    // 規劃移動路線
    public void EnemyMovePathFinding(GameObject enemy) 
    {
        bool isAlert = enemy.GetComponent<EnemyScript>().isAlert; // 警戒狀態
        Grid grid = GameObject.FindWithTag("MoveGrid").GetComponent<Grid>(); // 移動網格
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>(); 
        Vector3Int currentCell = grid.WorldToCell(enemy.transform.position); // 物件當前所在網格座標
        GameObject player = GameObject.FindWithTag("Player"); // 玩家物件
        List<Vector3Int> moveDirections = enemy.GetComponent<EnemyScript>().SetMoveModeDirection(enemy.GetComponent<EnemyScript>().moveMode); // 移動模式

        // 計算敵人的下一步該怎麼走
        if (!isAlert) // 非警戒狀態
        {
            int moveIndex = 0;
            while (moveIndex < 5)
            {
                currentCell = grid.WorldToCell(enemy.transform.position);
                Vector3Int direction = moveDirections[moveIndex];
                Vector3 moveTo = (grid.GetCellCenterWorld(currentCell + direction)); // 目的地
                NavMeshPath path = new NavMeshPath();
                // Debug.Log(NavMesh.CalculatePath(enemy.transform.position, moveTo, NavMesh.AllAreas, path));
                if (NavMesh.CalculatePath(enemy.transform.position, moveTo, NavMesh.AllAreas, path)) // 計算路徑
                {
                    // Debug.Log(GridManager.Instance.IsOccupied(currentCell + direction));
                    if (path.status == NavMeshPathStatus.PathComplete &&
                        !GridManager.Instance.IsOccupied(currentCell + direction) || direction == Vector3Int.zero)
                    {
                        GridManager.Instance.UpdateGameObjectFromMoveGrid(enemy, currentCell + direction);
                        enemy.GetComponent<EnemyScript>().targetPosition = moveTo;
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, 1f);
                        agent.SetPath(path); // 移動到目的地
                        break;
                        // else 做其他行為
                    }
                }
                
                moveIndex++;
            }
        }
        else
        {
            // 繪製到玩家的最短路徑
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(enemy.transform.position, player.transform.position, NavMesh.AllAreas, path))
            {
                Vector3[] corners = path.corners; // 儲存路徑的轉折點
                if (corners != null && corners.Length > 1)
                {
                    Vector3Int bestDir = new Vector3Int();
                    Vector3 dir = corners[1] - corners[0]; // 只看出發點與第一個轉折點的向量
                    dir.y = 0;
                    Vector2 flatDir = new Vector2(dir.x, dir.z).normalized;
                    // 確認移動方向 (上下左右)
                    if (Mathf.Abs(flatDir.x) > Mathf.Abs(flatDir.y))
                    {
                        bestDir = flatDir.x > 0 ? Vector3Int.right : Vector3Int.left;
                    }
                    else
                    {
                        bestDir = flatDir.y > 0 ? Vector3Int.forward : Vector3Int.back;
                    }

                    Vector3 moveTo = (grid.GetCellCenterWorld(currentCell + bestDir)); // 以網格座標中心轉換為世界座標
                    // 繪製到下一格的最短路徑
                    NavMeshPath finalPath = new NavMeshPath();
                    if (NavMesh.CalculatePath(enemy.transform.position, moveTo, NavMesh.AllAreas, finalPath) ||
                        GridManager.Instance.IsOccupied(grid.WorldToCell(moveTo)))
                    {
                        if (path.status == NavMeshPathStatus.PathComplete)
                        {
                            GridManager.Instance.UpdateGameObjectFromMoveGrid(enemy, grid.WorldToCell(moveTo));
                            enemy.GetComponent<EnemyScript>().targetPosition = moveTo;
                            agent.SetPath(finalPath); // 移動到目的地
                        }
                    }
                }
            }
        }
    }
}