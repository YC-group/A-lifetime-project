using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

/// <summary>
/// 敵人行為控制 - Jerry0401
/// </summary>
///
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Enemy Navigation", story: "[Enemy] navigates to [Target]", category: "Action", id: "e475d918b5e278090a578e4d8d6ed420")]
public partial class EnemyNavigationAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Enemy;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    private NavMeshAgent agent;
    private Vector3Int currentCell;
    private Vector3Int targetCurrentCell;
    private Grid grid;

    protected override Status OnStart()
    {
        // Debug.Log(Enemy.Value.name + "activate");
        grid = GameObject.FindWithTag("MoveGrid").GetComponent<Grid>();
        agent = Enemy.Value.GetComponent<NavMeshAgent>(); 
        currentCell = grid.WorldToCell(Enemy.Value.transform.position); // 定義敵人所在的網格座標
        targetCurrentCell = grid.WorldToCell(Target.Value.transform.position); // 定義玩家所在的網格座標
        
        // 計算敵人的下一步該怎麼走
        // 繪製到玩家的最短路徑
        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(Enemy.Value.transform.position, Target.Value.transform.position, NavMesh.AllAreas, path))
        {
            return Status.Failure;
        }
        Vector3[] corners = path.corners; // 儲存路徑的轉折點
        Vector3 moveTo;
        
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
            moveTo = (grid.GetCellCenterWorld(currentCell + bestDir)); // 以網格座標中心轉換為世界座標
            // 繪製到下一格的最短路徑
            NavMeshPath finalPath = new NavMeshPath();
            if (!NavMesh.CalculatePath(Enemy.Value.transform.position, moveTo, NavMesh.AllAreas, finalPath))
            {
                return Status.Failure;
            }
            agent.SetPath(finalPath); // 移動到目的地
        }
        
        return Status.Success;
    }
    
}