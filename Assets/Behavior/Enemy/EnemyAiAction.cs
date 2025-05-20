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
[NodeDescription(name: "EnemyAI", story: "[Self] [GridMode] navigates to [Target]", category: "Action",
    id: "e475d918b5e278090a578e4d8d6ed420")]
public partial class EnemyAiAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Grid> GridMode;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    private NavMeshAgent agent;
    private Vector3Int currentCell;
    private Vector3Int targetCurrentCell;
    private List<Vector3Int> directions;

    protected override Status OnStart()
    {
        agent = Self.Value.GetComponent<NavMeshAgent>(); 
        currentCell = GridMode.Value.WorldToCell(Self.Value.transform.position); // 定義敵人所在的網格座標
        targetCurrentCell = GridMode.Value.WorldToCell(Target.Value.transform.position); // 定義玩家所在的網格座標
        directions = new List<Vector3Int> // 定義移動方向
        {
            Vector3Int.right,
            Vector3Int.left,
            Vector3Int.forward,
            Vector3Int.back
        };
        
        MoveToNextGridTowardsPlayer();
        
        return Status.Success;
    }
    
    public void MoveToNextGridTowardsPlayer() // 計算敵人的下一步該怎麼走
    {   
        // 繪製路徑
        NavMeshPath path = new NavMeshPath(); 
        NavMesh.CalculatePath(Self.Value.transform.position, Target.Value.transform.position, NavMesh.AllAreas, path);
        Vector3[] corners = path.corners; // 儲存路徑的轉折點
        Vector3 moveTo;
        
        if (corners != null && corners.Length > 1)
        {
            Vector3Int bestDir = new Vector3Int();
            Vector3 dir = corners[1] - corners[0]; // 只看出發點與第一個轉折點的向量
            dir.y = 0;

            Vector2 flatDir = new Vector2(dir.x, dir.z).normalized;
            // 分為四個方向
            if (Mathf.Abs(flatDir.x) > Mathf.Abs(flatDir.y))
            {
                bestDir = flatDir.x > 0 ? Vector3Int.right : Vector3Int.left;
            }
            else
            {
                bestDir = flatDir.y > 0 ? Vector3Int.forward : Vector3Int.back;
            }
            moveTo = (GridMode.Value.GetCellCenterWorld(currentCell + bestDir)); // 以網格座標中心轉換為世界座標
            agent.SetDestination(moveTo); // 移動到目的地
        }
        
    }
}