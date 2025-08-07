using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class EnemyIdleState : EnemyState
{
    public EnemyIdleState(EnemyScript enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {
        
    }

    public override void Enter()
    {
       
    }

    public override void Update()
    {
        if (enemy.isMoving)
        {
            Debug.LogWarning("The enemy is moving !");
        }
        
        int moveIndex = 0;
        Coroutine rotationCoroutine = enemy.rotationCoroutine;
        while (moveIndex < 5)
        {
            Vector3Int currentCell = GridManager.GetInstance().moveGrid.WorldToCell(enemy.transform.position);
            Vector3Int direction = enemy.GetMoveModeDirectionMode()[moveIndex];
            Vector3 moveTo = (GridManager.GetInstance().moveGrid.GetCellCenterWorld(currentCell + direction));
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(enemy.transform.position, moveTo, NavMesh.AllAreas, path) &&
                GridManager.GetInstance().moveGrid.GetCellCenterWorld(PlayerScript.GetInstance().currentCell) != moveTo)
            {
                if (path.status == NavMeshPathStatus.PathComplete &&
                    !GridManager.GetInstance().IsOccupied(currentCell + direction) && path.corners.Length < 3 ||
                    direction == Vector3Int.zero)
                {
                    enemy.targetPosition = moveTo;
                    if (rotationCoroutine != null) // 確保只有一個轉向行為
                    {
                        enemy.StopCoroutine(rotationCoroutine);
                    }

                    enemy.rotationCoroutine =
                        enemy.StartCoroutine(enemy.SmoothRotation(direction));
                    GridManager.GetInstance().UpdateGameObjectFromMoveGrid(enemy.gameObject, currentCell + direction);
                    // Debug.Log(moveTo);
                    enemy.StartCoroutine(enemy.SmoothMove(moveTo)); // 移動到目的地
                    break;
                    // else 做其他行為
                }
            }

            moveIndex++;
        }
    }
}
