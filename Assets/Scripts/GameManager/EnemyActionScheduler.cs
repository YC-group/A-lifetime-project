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
    private GridManager gridManager;
    private GameManager gameManager;
    private PlayerScript player;
    private HealthPointsScript healthPointsScript;
    
    void Start()
    {
        isMoving = false;
        gridManager = GridManager.GetInstance();
        gameManager = GameManager.GetInstance();
        player = PlayerScript.GetInstance(); // 玩家物件
        healthPointsScript = HealthPointsScript.GetInstance();
        
        foreach (var enemy in EnemyGameObjects)
        {
            gridManager.AddGameObjectToMoveGrid(enemy); // 將敵人物件存入網格陣列
        }
    }

    void Update()
    {
        currentRound = gameManager.GetCurrentRound(); // 確定現在回合
    }

    void FixedUpdate() 
    {
        // HACK: 重複檢查可能導致效能問題
        if (RoundState.EnemyTurn == currentRound)
        {
            // GameObject.FindWithTag("GameManager").GetComponent<GameManager>().SetToNextRound();
            if (!isMoving) // 是否正在移動
            {
                EnemyMoveSchedule(); // 廣播敵人行動
            }
            if (CheckEnemyArrived()) // 確定敵人動作結束
            {
                gameManager.SetToNextRound();
            }
        }
    }

    // 確定敵人動作結束
    private bool CheckEnemyArrived()
    {
        int actionComplete = 0; // 計算場景中動作結束的敵人數量
        if (EnemyGameObjects != null)
        {
            foreach (var enemy in EnemyGameObjects)
            {
                if (enemy.GetComponent<EnemyScript>().targetPosition == enemy.transform.position)
                {
                    actionComplete++;
                }
            }

            if (actionComplete == EnemyGameObjects.Length) // 判斷行動是否結束
            {
                // Debug.Log("Action Complete");
                isMoving = false;
                return true;
            }
            return false;
        }
        return true; // 場景中沒有敵人
    }

    // HACK: 敵人移動規劃
    // 之後會把 FindGameObjectsWithTag 改掉
    // 因為不適用於每個房間
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
                // Invoke("EnemyMovePathFinding", 1f);
                EnemyMovePathFinding(enemy); // 規劃移動路線
            }
        }
    }
    
    // 規劃敵人移動路線
    // NOTE: 使用 NavMesh System，會有大量計算
    private void EnemyMovePathFinding(GameObject enemy)
    {
        EnemyScript enemyScript = enemy.GetComponent<EnemyScript>();
        bool isAlert = enemyScript.isAlert; // 警戒狀態
        Grid moveGrid = GridManager.GetInstance().moveGrid; // 移動網格
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>(); 
        Vector3Int currentCell = moveGrid.WorldToCell(enemy.transform.position); // 物件當前所在網格座標
        
        List<Vector3Int> moveDirections = enemyScript.SetMoveModeDirection(enemyScript.moveMode); // 移動模式
        Coroutine rotationCoroutine = enemyScript.rotationCoroutine;

        // 計算敵人的下一步該怎麼走
        if (!isAlert) // 非警戒狀態
        {
            int moveIndex = 0;
            while (moveIndex < 5)
            {
                currentCell = moveGrid.WorldToCell(enemy.transform.position);
                Vector3Int direction = moveDirections[moveIndex];
                Vector3 moveTo = (moveGrid.GetCellCenterWorld(currentCell + direction)); // 目的地
                NavMeshPath path = new NavMeshPath();
                // Debug.Log(NavMesh.CalculatePath(enemy.transform.position, moveTo, NavMesh.AllAreas, path));
                if (NavMesh.CalculatePath(enemy.transform.position, moveTo, NavMesh.AllAreas, path) && moveGrid.GetCellCenterWorld(player.currentCell) != moveTo)
                {
                    // Debug.Log("corners : " + path.corners.Length);
                    // Debug.Log(GridManager.Instance.IsOccupied(currentCell + direction));
                    if (path.status == NavMeshPathStatus.PathComplete &&
                        !gridManager.IsOccupied(currentCell + direction) && path.corners.Length < 3|| direction == Vector3Int.zero)
                    {
                        enemyScript.targetPosition = moveTo;
                        if (rotationCoroutine != null) // 確保只有一個轉向行為
                        {
                            StopCoroutine(rotationCoroutine);
                        }
                        enemyScript.rotationCoroutine = StartCoroutine(enemyScript.SmoothRotation(direction));
                        gridManager.UpdateGameObjectFromMoveGrid(enemy, currentCell + direction);
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

                    Vector3 moveTo = (moveGrid.GetCellCenterWorld(currentCell + bestDir)); // 以網格座標中心轉換為世界座標
                    // 繪製到下一格的最短路徑
                    NavMeshPath finalPath = new NavMeshPath();
                    // Debug.Log(!gridManager.IsOccupied(moveTo));
                    if (NavMesh.CalculatePath(enemy.transform.position, moveTo, NavMesh.AllAreas, finalPath) &&
                        !gridManager.IsOccupied(moveTo))
                    {
                        if (path.status == NavMeshPathStatus.PathComplete)
                        {
                            if (!enemyScript.isStun && moveGrid.GetCellCenterWorld(player.currentCell).Equals(moveTo))
                            {
                                player.hp = healthPointsScript.TakeMeleeDamage(player.hp);
                            }
                            enemyScript.targetPosition = moveTo;
                            if (rotationCoroutine != null) // 確保只有一個轉向行為
                            {
                                StopCoroutine(rotationCoroutine);
                            }
                            enemyScript.rotationCoroutine = StartCoroutine(enemyScript.SmoothRotation(bestDir));
                            gridManager.UpdateGameObjectFromMoveGrid(enemy, moveGrid.WorldToCell(moveTo));
                            agent.SetPath(finalPath); // 移動到目的地
                        }
                    }
                }
            }
        }
    }
}