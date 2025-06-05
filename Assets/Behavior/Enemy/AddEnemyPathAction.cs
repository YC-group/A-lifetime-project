using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using UnityEngine.EventSystems;

/// <summary>
/// 敵人行為控制 - Jerry0401
/// </summary>
///
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Add Enemy Path", story: "[Enemy] navigates to [Target] or move", category: "Action", id: "e475d918b5e278090a578e4d8d6ed420")]
public partial class AddEnemyPathAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Enemy;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    private NavMeshAgent agent;
    private Vector3Int currentCell;
    private Vector3Int targetCurrentCell;
    private Grid grid;
    private bool isAlert;
    private List<Vector3Int> moveDirections;
    private GameManager gameManager;
    private EnemyActionScheduler enemyActionScheduler;

    protected override Status OnStart()
    {
        moveDirections = getMoveModeDirection(Enemy.Value.GetComponent<EnemyScript>().moveMode);
        isAlert = Enemy.Value.GetComponent<EnemyScript>().IsAlert;
        grid = GameObject.FindWithTag("MoveGrid").GetComponent<Grid>();
        agent = Enemy.Value.GetComponent<NavMeshAgent>();
        currentCell = grid.WorldToCell(Enemy.Value.transform.position); // 定義敵人所在的網格座標
        targetCurrentCell = grid.WorldToCell(Target.Value.transform.position); // 定義玩家所在的網格座標
        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        enemyActionScheduler = GameObject.FindWithTag("GameManager").GetComponent<EnemyActionScheduler>();
        
        // 計算敵人的下一步該怎麼走
        if (!isAlert) // 非警戒狀態
        {
            List<Vector3Int> moveDirectionsTmp = new List<Vector3Int>(moveDirections);
            int moveIndex = 0;
            while (moveIndex < 5)
            {
                Vector3Int direction = moveDirections[moveIndex];
                Vector3 moveTo = (grid.GetCellCenterWorld(currentCell + direction)); // 目的地
                // Debug.Log("Move to : " + moveTo);
                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(Enemy.Value.transform.position, moveTo, NavMesh.AllAreas, path)) // 計算路徑
                {
                    if (path.status == NavMeshPathStatus.PathComplete && !enemyActionScheduler.EnemyNextPositions.ContainsKey(Enemy.Value))
                    {       
                        enemyActionScheduler.EnemyNextPositions.Add(Enemy.Value, moveTo);// 加入到移動陣列
                        Enemy.Value.GetComponent<EnemyScript>().SetPath(path); // 暫存路徑
                        break;
                        // else 做其他行為
                        // agent.SetPath(path); // 移動到目的地
                        // break;
                    }
                }
                moveIndex++;
            }
        }
        else
        {
            // 繪製到玩家的最短路徑
            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(Enemy.Value.transform.position, Target.Value.transform.position, NavMesh.AllAreas, path))
            {
                return Status.Failure;
            }
            
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
                if (!NavMesh.CalculatePath(Enemy.Value.transform.position, moveTo, NavMesh.AllAreas, finalPath))
                {
                    return Status.Failure;
                }
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    enemyActionScheduler.EnemyNextPositions.Add(Enemy.Value, moveTo);// 加入到移動陣列
                    Enemy.Value.GetComponent<EnemyScript>().SetPath(path); // 暫存路徑
                    // else 做其他行為
                    // agent.SetPath(finalPath); // 移動到目的地
                }
            }
        }
        return Status.Success;
    }

    private List<Vector3Int> getMoveModeDirection(MoveMode mode)
    {
        switch (mode)
        {
            case MoveMode.DownRightUpLeft:
                return new List<Vector3Int>(){ Vector3Int.back, Vector3Int.right, Vector3Int.forward, Vector3Int.left, Vector3Int.zero };
            case MoveMode.UpLeftDownRight:
                return new List<Vector3Int>(){ Vector3Int.forward, Vector3Int.left, Vector3Int.back, Vector3Int.right, Vector3Int.zero };
            default:
                return new List<Vector3Int>(){ Vector3Int.back, Vector3Int.right, Vector3Int.forward, Vector3Int.left, Vector3Int.zero };
        }
    }
}