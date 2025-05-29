using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 角色移動腳本 - Jerry0401
/// </summary>

public class PlayerScript : MonoBehaviour
{
    [SerializeField] private PlayerData playerSO; // 序列化玩家物件
    private bool isMoving = false; // 判斷玩家是否正在移動
    // private Rigidbody rb;
    private Vector2 moveVector;
    public Grid grid; // 網格系統
    private Vector3Int currentCell;
    public bool FREEMOVE = false; // 測試移動用，會讓回合維持在玩家回合
    private InputSystemActions inputActions; // InputSystem 的 Action map
    private GameManager gameManager;
    public List<ItemScript> PocketList{get; set;}

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        grid = GameObject.FindWithTag("MoveGrid").GetComponent<Grid>();
        currentCell = grid.WorldToCell(transform.position);
        transform.position = grid.GetCellCenterWorld(currentCell);
        PocketList = new List<ItemScript>();
        // 註冊移動行為
        inputActions = new InputSystemActions();
        inputActions.Player.Move.performed += Move;
        inputActions.Player.Move.canceled += Move;
        inputActions.Enable();
    }

    void Update()
    {

    }


    public void Move(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !isMoving && gameManager.GetCurrentRound().Equals(RoundState.PlayerTurn))
        {
            moveVector = ctx.ReadValue<Vector2>();
            //Debug.Log("輸入向量：" + moveVector);

            if (moveVector != Vector2.zero && moveVector.x % 1f == 0f && moveVector.y % 1f == 0f)
            {
                Vector3Int direction = new Vector3Int(Mathf.RoundToInt(moveVector.x), 0, Mathf.RoundToInt(moveVector.y));
                var detection = DetectBuildingsAndEnemies();//先偵測
                var buildDict = detection["buildDict"] as Dictionary<string, List<Building>>; //取出牆值
                var enemyDict = detection["enemyDict"] as Dictionary<string, List<EnemyScript>>; //取出敵人
                int step = moveStepCount(buildDict); //計算移動格數
                if (step > 0) 
                {
                    currentCell += direction * playerSO.moveDistance * step;
                    Vector3 dest = grid.GetCellCenterWorld(currentCell);
                    enemyCheck(enemyDict, step);
                    StartCoroutine(SmoothMove(dest));
                }
                return;
                
            }
        }
    }


    IEnumerator SmoothMove(Vector3 destination) // 使用迭代做平滑移動
    {
        isMoving = true;
        Vector3 start = transform.position;
        float elapsed = 0f; // 已經過時間
        float duration = playerSO.moveTime; // 移動時間，可調整

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, destination, elapsed / duration); // 利用重複呼叫線性插值結果做出平滑移動
            elapsed += Time.deltaTime; // 每 Frame 經過時間
            yield return null; // 結束這一 Frame 的 Coroutine
        }

        transform.position = destination; // 確保精準落格
        isMoving = false;

        if (!FREEMOVE)
        {
            gameManager.SetCurrentRound(RoundState.EnemyTurn); // 敵人回合開始
        }

    }


    /// <summary>
    /// 偵測角色前方兩格內的 Building 物件（每格一組）- mobias
    /// </summary>
    [SerializeField] public Vector3 buildDetectionBox = new Vector3(0.9f, 0.1f, 0.8f) ;
    [SerializeField] public Vector3 moveDetectionBox = new Vector3(0.9f, 0.1f, 0.9f) ;

    [Range(0f, 3f)]
    [SerializeField] float overlapDetectionBoxYOffset = 0.5f;
    private Dictionary<string, object> DetectBuildingsAndEnemies() //道具偵測可由此修改
    {
        Dictionary<string, List<Building>> buildDict = new Dictionary<string, List<Building>>
            {
                { "build1", new List<Building>() },
                { "build2", new List<Building>() },
                { "move1", new List<Building>() },
                { "move2", new List<Building>() }
            };

        Dictionary<string, List<EnemyScript>> enemyDict = new Dictionary<string, List<EnemyScript>>
            {
                { "enemy1", new List<EnemyScript>() },
                { "enemy2", new List<EnemyScript>() }
            };

        Dictionary<string, object> result = new()
            {
                { "buildDict", buildDict },
                { "enemyDict", enemyDict }
            };

        // ✅ 取得方向（用 moveVector）與 normalized 方向（偏移用）
        Vector3Int forwardGridDir = new Vector3Int(Mathf.RoundToInt(moveVector.x), 0, Mathf.RoundToInt(moveVector.y));
        Vector3 forwardDir = new Vector3(forwardGridDir.x, 0, forwardGridDir.z).normalized;

        for (int i = 1; i <= 2; i++)
        {
            // ========= moveX: 格子中心 OverlapBox =========
            Vector3Int checkCell = currentCell + forwardGridDir * i;
            Vector3 worldPos = grid.GetCellCenterWorld(checkCell)+Vector3.up* overlapDetectionBoxYOffset;

            Collider[] hitsA = Physics.OverlapBox(worldPos, moveDetectionBox); // 格子中心偵測
            foreach (var hit in hitsA)
            {
                Building b = hit.GetComponent<Building>();
                if (b != null)
                {
                    buildDict[$"move{i}"].Add(b);
                }

                //EnemyScript enemy = hit.GetComponent<EnemyScript>();
                //if (enemy != null)
                //{
                //    enemyDict[$"enemy{i}"].Add(enemy);
                //}

            }

            // ========= buildX: 偏移位置偵測 =========
            Vector3 detectCenter = worldPos - forwardDir * 1.5f + Vector3.up* overlapDetectionBoxYOffset;
            Quaternion rotation = Quaternion.LookRotation(forwardDir);

            Collider[] hitsB = Physics.OverlapBox(detectCenter, buildDetectionBox, rotation);
            foreach (var hit in hitsB)
            {
                Building b = hit.GetComponent<Building>();
                if (b != null)
                {
                    buildDict[$"build{i}"].Add(b);
                }
            }

            // --------- ✅搜尋沒有 Collider 的敵人（用 Tag） ----------
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemyObj in enemies)
            {
                float dist = Vector3.Distance(enemyObj.transform.position, worldPos);
                if (dist < 0.9f) // 判定範圍可依格子大小調整
                {
                    EnemyScript e = enemyObj.GetComponent<EnemyScript>();
                    if (e != null && !enemyDict[$"enemy{i}"].Contains(e))
                    {
                        enemyDict[$"enemy{i}"].Add(e);
                    }
                }
            }
        }   

    return result;
}


    //繪製偵測格子 - mobias
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (grid == null) return;

        // 取得角色目前格子
        Vector3Int currentCellGizmo = grid.WorldToCell(transform.position);

        // 根據移動向量計算方向（X,Z）
        Vector3Int forwardGridDir = new Vector3Int(Mathf.RoundToInt(moveVector.x), 0, Mathf.RoundToInt(moveVector.y));
        Vector3 forwardDir = new Vector3(forwardGridDir.x, 0, forwardGridDir.z).normalized;

        // --------- 🔴 原本前方兩格的紅色格子 ----------
        Gizmos.color = Color.red;

        for (int i = 1; i <= 2; i++)
        {
            Vector3Int targetCell = currentCellGizmo + forwardGridDir * i;
            Vector3 cellCenter = grid.GetCellCenterWorld(targetCell)+ Vector3.up* overlapDetectionBoxYOffset;
            Gizmos.DrawCube(cellCenter, moveDetectionBox);
        }

        // --------- 🔵 新增偵測區塊（藍色框） ----------
        // 前方第一格格子中心
        for (int i = 1; i <= 2; i++)
        {
            Vector3Int firstFrontCell = currentCellGizmo + forwardGridDir * i;
            Vector3 frontCenter = grid.GetCellCenterWorld(firstFrontCell);

            // 保護：避免 zero 向量導致 Quaternion.LookRotation 出錯
            if (forwardDir == Vector3.zero) return;

            // 往相反方向退後 1.5 單位（從中心點）
            Vector3 detectCenter = frontCenter - forwardDir * 1.5f + Vector3.up* overlapDetectionBoxYOffset;;

            // 設定偵測框的大小與旋轉

            Quaternion rotation = Quaternion.LookRotation(forwardDir);

            // 畫出藍色框
            Gizmos.color = Color.blue;
            Gizmos.matrix = Matrix4x4.TRS(detectCenter, rotation, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, buildDetectionBox);

        }


    }
#endif


    /// <summary>
    /// 根據目前建築物資訊，遞迴決定最多可以往前移動幾格（最多 2 格）- mobias
    /// </summary>

    private int moveStepCount(Dictionary<string, List<Building>> buildings,  int step = 0)
    {
        // 若已經超過 1 步（即將進入第 3 步），不允許，回傳 0（代表失敗）
        if (step > 1)
            return 0;

        // 取得目前 step + 1 格的資料（因為 index 是從 1 開始）
        bool isCrossBuild = !buildings[$"build{step + 1}"].Any(b => b.buildingSO != null && !b.buildingSO.isCrossable);
        bool isCrossMove = !buildings[$"move{step + 1}"].Any(b => b.buildingSO != null && !b.buildingSO.isCrossable);
        bool hasWallMove = buildings[$"move{step + 1}"].Count > 0;

        //bool hasEnemy = enemys[$"enemy{step + 1}"].Count > 0;
        //print(hasEnemy);
        if (isCrossBuild && !hasWallMove) // ✅ 第一種情況：該格沒有牆，可以直接通過，step+1
        {
            step++;
        }
        else if (isCrossBuild && hasWallMove && isCrossMove) // ✅ 第二種情況：該格有牆但可以穿越，繼續往下一格判斷（遞迴）
        {
            step = moveStepCount(buildings, step + 1);
        }
        else  // ❌ 無法穿越（either build or move fail）
        {
            step = 0;
        }

        return step;
    }

    void enemyCheck(Dictionary<string, List<EnemyScript>> enemys, int step)
    {
        if ( step == 0 ) return;
        foreach (var enemy in enemys[$"enemy{step}"])
        {
            print("觸發近戰攻擊條件");
            enemy.DestroyEnemy();
        }
    }

}