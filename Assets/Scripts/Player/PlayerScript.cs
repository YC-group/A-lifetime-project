using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

/// <summary>
/// 角色移動腳本 - Jerry0401
/// </summary>

[RequireComponent(typeof(CharacterController))]
public class PlayerScript : MonoBehaviour
{
    private static PlayerScript Instance;
   
    public bool FREEMOVE = false; // 測試移動用，會讓回合維持在玩家回合
    public List<ItemScript> pocketList;
    public Vector3Int currentCell; // 當下網格位置
    public RangeWeapon currentCard;
    [SerializeField] private PlayerData playerSO; // 序列化玩家物件
    
    private InputSystemActions inputActions; // InputSystem 的 Action map
    private bool isMoving = false; // 判斷玩家是否正在移動
    private Vector2 moveVector; // 移動方向
    private GameManager gameManager; // 遊戲系統
    private GridManager gridManager; // 網格系統
    private Grid moveGrid; // 移動網格


    public static PlayerScript GetInstance()  // Singleton
    {
        if (Instance == null)
        {
            Instance = GameObject.FindAnyObjectByType<PlayerScript>();
            if (Instance == null)
            {
                Debug.LogError("No PlayerScript found");
                return null;
            }
        }
        return Instance;
    }

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("已存在其他 Player 實例，將刪除此物件。");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    private void OnDestroy()
    {
        // Singleton
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        gameManager = GameManager.GetInstance();
        gridManager = GridManager.GetInstance();
        moveGrid = gridManager.moveGrid;
        currentCell = moveGrid.WorldToCell(transform.position);
        transform.position = moveGrid.GetCellCenterWorld(currentCell);
        pocketList = new List<ItemScript>();
        // 註冊移動行為
        inputActions = new InputSystemActions();
        inputActions.Player.Move.performed += Move;
        inputActions.Player.Move.canceled += Move;
        inputActions.Player.Skip.performed += Skip;
        inputActions.Player.Skip.canceled += Skip;
        inputActions.Player.Fire.performed += ctx =>
        {
            if (currentCard != null && currentCard.selectEnemy)
                currentCard.Fire();
        };

        inputActions.Player.Cancel.performed += ctx =>
        {
            if (currentCard != null && currentCard.selectEnemy)
                currentCard.CancelAttackAndRestore();
        };

        inputActions.Player.Select.performed += ctx =>
        {

            if (currentCard != null && currentCard.selectEnemy)
                currentCard.SelectTarget();
        };
        inputActions.Enable();
    }

    // Space 跳過行為
    // NOTE: 使用 InputAction
    public void Skip(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && gameManager.GetCurrentRound().Equals(RoundState.PlayerTurn))
        {
            if (gridManager.IsOccupiedByEnemy(moveGrid.WorldToCell(transform.position)))
            {
                GameObject enemy = gridManager.GetGameObjectFromMoveGrid(moveGrid.WorldToCell(transform.position));
                if (enemy.GetComponent<EnemyScript>().isStun)
                {
                    enemy.GetComponent<EnemyScript>().DestroyEnemy();
                }
                else
                {
                    Debug.Log("Game Over");
                    // TODO: 玩家死亡
                }
            }
            gameManager.SetToNextRound();
        }
    }


    // WASD 移動行為
    // NOTE: 使用 InputAction
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
                var doorDict = detection["doorDict"] as Dictionary<string, List<Door>>;
                int step = moveStepCount(buildDict); //計算移動格數
                if (step > 0) 
                {
                    Door door = null;
                    foreach (KeyValuePair<string, List<Door>> kvp in doorDict)
                    {
                        foreach(Door doorComponent in kvp.Value)
                        {
                            if(door != null)
                            {
                                if(door.gameObject != doorComponent.gameObject)
                                {
                                    Debug.Log($"{door.gameObject.name};{doorComponent.gameObject.name}");
                                    Debug.LogError("非重複的門");
                                }
                            }
                            else
                            {
                                door = doorComponent;
                            }
                        }
                    }
                    if(door != null) door.OpenDoor();
                    
                    currentCell += direction * playerSO.moveDistance * step;
                    // gridManager.UpdateGameObjectFromMoveGrid(this.gameObject, currentCell); // 更新在網格系統的所在位置
                    Vector3 dest = moveGrid.GetCellCenterWorld(currentCell);
                    enemyCheck(enemyDict, step);
                    StartCoroutine(SmoothMove(dest));
                    if (!FREEMOVE)
                    {
                        gameManager.SetToNextRound(); // 敵人回合開始
                    }
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
    }
    
    // HACK: 近戰攻擊行為
    public void MeleeAttack(EnemyScript enemy)
    {
        if (enemy.isStun)
        {
            enemy.DestroyEnemy();
        }
        else
        {
            enemy.isStun = true;
            enemy.stunRound = gameManager.GetAfterRoundsCounts();
            Debug.Log("擊暈敵人");
        }
    }


    /// <summary>
    /// 偵測角色前方兩格內的 Building 物件（每格一組）- mobias
    /// </summary>
    [SerializeField] public Vector3 buildDetectionBox = new Vector3(0.9f, 1.5f, 0.8f) ;
    [SerializeField] public Vector3 moveDetectionBox = new Vector3(0.9f, 1.5f, 0.9f) ;

    [Range(0f, 3f)]
    [SerializeField] float overlapDetectionBoxYOffset = 2f;
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

        Dictionary<string, List<ItemScript>> itemDict = new Dictionary<string, List<ItemScript>>
            {
                { "itemMove1", new List<ItemScript>() },
                { "itemMove2", new List<ItemScript>() },
                { "itemBuild1", new List<ItemScript>() },
                { "itemBuild2", new List<ItemScript>() },
            };

        Dictionary<string, List<Door>> doorDict = new Dictionary<string, List<Door>>
        {
            {"door1", new List<Door>()}
        };

        Dictionary<string, object> result = new()
            {
                { "buildDict", buildDict },
                { "enemyDict", enemyDict },
                { "itemDict", itemDict },
                { "doorDict", doorDict}
            };

        // ✅ 取得方向（用 moveVector）與 normalized 方向（偏移用）
        Vector3Int forwardGridDir = new Vector3Int(Mathf.RoundToInt(moveVector.x), 0, Mathf.RoundToInt(moveVector.y));
        Vector3 forwardDir = new Vector3(forwardGridDir.x, 0, forwardGridDir.z).normalized;

        for (int i = 1; i <= 2; i++)
        {
            // ========= moveX: 格子中心 OverlapBox =========
            Vector3Int checkCell = currentCell + forwardGridDir * i;
            Vector3 worldPos = moveGrid.GetCellCenterWorld(checkCell)+Vector3.up* overlapDetectionBoxYOffset;

            Collider[] hitsA = Physics.OverlapBox(worldPos, moveDetectionBox); // 格子中心偵測
            foreach (var hit in hitsA)
            {
                Building b = hit.GetComponent<Building>();
                if (b != null)
                {
                    buildDict[$"move{i}"].Add(b);
                }

                EnemyScript enemy = hit.GetComponent<EnemyScript>();
                if (enemy != null)
                {
                    enemyDict[$"enemy{i}"].Add(enemy);
                }

                ItemScript item = hit.GetComponent<ItemScript>();
                if (item != null)
                {
                    itemDict[$"itemMove{i}"].Add(item);
                }

            }

            // ========= buildX: 偏移位置偵測 =========
            Vector3 detectCenter = worldPos - forwardDir * 1.5f + Vector3.up * overlapDetectionBoxYOffset;
            Quaternion rotation = Quaternion.LookRotation(forwardDir);

            Collider[] hitsB = Physics.OverlapBox(detectCenter, buildDetectionBox, rotation);
            foreach (var hit in hitsB)
            {
                Building b = hit.GetComponent<Building>();
                if (b != null)
                {
                    buildDict[$"build{i}"].Add(b);
                }

                ItemScript item = hit.GetComponent<ItemScript>();
                if (item != null)
                {
                    itemDict[$"itemBuild{i}"].Add(item);
                }

                if(i == 1)
                {
                    Door door = hit.GetComponent<Door>();
                    if (door != null)
                    {
                        doorDict[$"door{i}"].Add(door);
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
        if (moveGrid == null) return;

        // 取得角色目前格子
        Vector3Int currentCellGizmo = moveGrid.WorldToCell(transform.position);

        // 根據移動向量計算方向（X,Z）
        Vector3Int forwardGridDir = new Vector3Int(Mathf.RoundToInt(moveVector.x), 0, Mathf.RoundToInt(moveVector.y));
        Vector3 forwardDir = new Vector3(forwardGridDir.x, 0, forwardGridDir.z).normalized;

        // --------- 🔴 原本前方兩格的紅色格子 ----------
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);

        for (int i = 1; i <= 2; i++)
        {
            Vector3Int targetCell = currentCellGizmo + forwardGridDir * i;
            Vector3 cellCenter = moveGrid.GetCellCenterWorld(targetCell)+ Vector3.up* overlapDetectionBoxYOffset;
            Gizmos.DrawCube(cellCenter, moveDetectionBox);
        }

        // --------- 🔵 新增偵測區塊（藍色框） ----------
        // 前方第一格格子中心
        for (int i = 1; i <= 2; i++)
        {
            Vector3Int firstFrontCell = currentCellGizmo + forwardGridDir * i;
            Vector3 frontCenter = moveGrid.GetCellCenterWorld(firstFrontCell);

            // 保護：避免 zero 向量導致 Quaternion.LookRotation 出錯
            if (forwardDir == Vector3.zero) return;

            // 往相反方向退後 1.5 單位（從中心點）
            Vector3 detectCenter = frontCenter - forwardDir * 1.5f + Vector3.up* overlapDetectionBoxYOffset;;

            // 設定偵測框的大小與旋轉

            Quaternion rotation = Quaternion.LookRotation(forwardDir);

            // 畫出藍色框
            Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
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
        foreach (var enemy in enemys[$"enemy{step}"])
        {
            print("觸發近戰攻擊條件");
            MeleeAttack(enemy);
            // enemy.DestroyEnemy();
        }
    }

}