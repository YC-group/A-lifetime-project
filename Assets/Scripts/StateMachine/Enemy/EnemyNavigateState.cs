using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigateState : EnemyState
{
    public EnemyNavigateState(EnemyScript enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {
    }

    public override void Update()
    {
        if (enemy.isMoving)
        {
            Debug.LogError("The enemy is moving !");
        }

        if (!enemy.isAlert)
        {
            Debug.LogError("The enemy isn't alert !");
        }

        Vector3Int currentCell = GridManager.GetInstance().moveGrid.WorldToCell(enemy.transform.position);

        // 繪製到玩家的最短路徑
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(enemy.transform.position, PlayerScript.GetInstance().transform.position,
                NavMesh.AllAreas, path))
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

                Vector3 moveTo = (GridManager.GetInstance().moveGrid.GetCellCenterWorld(currentCell + bestDir)); // 以網格座標中心轉換為世界座標
                // 繪製到下一格的最短路徑
                NavMeshPath finalPath = new NavMeshPath();
                // Debug.Log(!gridManager.IsOccupied(moveTo));
                if (NavMesh.CalculatePath(enemy.transform.position, moveTo, NavMesh.AllAreas, finalPath) &&
                    !GridManager.GetInstance().IsOccupied(moveTo))
                {
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        if (!enemy.isStun && GridManager.GetInstance().moveGrid
                                .GetCellCenterWorld(PlayerScript.GetInstance().currentCell)
                                .Equals(moveTo))
                        {
                            // 近戰攻擊
                            PlayerScript.GetInstance().hp = HealthPointsScript.GetInstance()
                                .TakeMeleeDamage(PlayerScript.GetInstance().hp);
                        }

                        enemy.targetPosition = moveTo;
                        if (enemy.rotationCoroutine != null) // 確保只有一個轉向行為
                        {
                            enemy.StopCoroutine(enemy.rotationCoroutine);
                        }

                        enemy.rotationCoroutine =
                            enemy.StartCoroutine(enemy.SmoothRotation(bestDir));
                        GridManager.GetInstance()
                            .UpdateGameObjectFromMoveGrid(enemy.gameObject, GridManager.GetInstance().moveGrid.WorldToCell(moveTo));
                        enemy.StartCoroutine(enemy.SmoothMove(moveTo)); // 移動到目的地
                    }
                }
            }
        }
    }
}