using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 角色移動腳本 - Jerry0401
/// </summary>

public class PlayerScript : MonoBehaviour
{


    [SerializeField] public PlayerData playerObj; // 序列化玩家物件
    private bool isMoving = false; // 判斷玩家是否正在移動
    // private Rigidbody rb;
    private Vector2 moveVector;
    public Grid grid; // 網格系統
    private Vector3Int currentCell;
    public bool FREEMOVE = false; // 測試移動用，會讓回合維持在玩家回合
    private InputSystemActions inputActions; // InputSystem 的 Action map
 
    private void Start()
    {
        currentCell = grid.WorldToCell(transform.position);
        transform.position = grid.GetCellCenterWorld(currentCell);
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
        if (ctx.performed && !isMoving && GameManager.GameState.GetCurrentRound().Equals(RoundState.PlayerTurn))
        {
            moveVector = ctx.ReadValue<Vector2>();
            Debug.Log("輸入向量：" + moveVector);

            if (moveVector != Vector2.zero && moveVector.x % 1f == 0f && moveVector.y % 1f == 0f)
            {
                Vector3Int direction = new Vector3Int(Mathf.RoundToInt(moveVector.x), 0, Mathf.RoundToInt(moveVector.y));

                // ✅ 取得角色面前兩格中的建築物件（每格為一組）
                List<List<Building>> buildings = DetectBuildingsInFrontTwoTiles();

                bool firstIsHardwall = false;   // 紀錄第一格是否是硬牆
                bool secondHasWall = false;     // 紀錄第二格是否有建築物（不論類型）

                // ✅ 判斷第一格是否包含 Hardwall
                foreach (Building b in buildings[0])
                {
                    if (b.buildingObj != null && b.buildingObj.buildingType == BuildingData.BuildingType.Hardwall)
                    {
                        firstIsHardwall = true;
                        break;
                    }
                }

                // ✅ 判斷第二格是否有任意建築（不管類型）
                if (buildings.Count > 1 && buildings[1].Count > 0)
                {
                    secondHasWall = true;
                }
                // ✅ 決策邏輯：有硬牆則額外處理
                if (firstIsHardwall)
                {
                    if (secondHasWall)
                    {
                        // ⛔ 第一格是硬牆、第二格也有建築 → 停止移動
                        return;
                    }
                    else
                    {
                        //  第一格是硬牆、第二格空 → 跳過第一格，直接移動兩格
                        currentCell += direction * playerObj.moveDistance * 2;
                        Vector3 skipDestination = grid.GetCellCenterWorld(currentCell);
                        StartCoroutine(SmoothMove(skipDestination));
                        return;
                    }
                }

                // Debug：印出偵測物件
                //List<List<string>> detectedObjectsPerTile = DetectObjectsInFrontTwoTilesSeparated();
                //for (int i = 0; i < detectedObjectsPerTile.Count; i++)
                //{
                //    foreach (string objName in detectedObjectsPerTile[i])
                //    {
                //        Debug.Log($"第 {i + 1} 格偵測到物件：{objName}");
                //    }
                //}

                // 否則正常移動一格
                currentCell += direction * playerObj.moveDistance;
                Vector3 destination = grid.GetCellCenterWorld(currentCell);
                StartCoroutine(SmoothMove(destination));
            }
        }
    }


    IEnumerator SmoothMove(Vector3 destination) // 使用迭代做平滑移動
    {
        isMoving = true;
        Vector3 start = transform.position;
        float elapsed = 0f; // 已經過時間
        float duration = playerObj.moveTime; // 移動時間，可調整

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
            GameManager.GameState.SetCurrentRound(RoundState.EnemyTurn); // 敵人回合開始
        }
            
    }
    /// <summary>
    /// 射線偵測-mobias
    /// </summary>

    [Range(1f, 8f)]
    [SerializeField] float lineDistance = 1f; //射線長度
    [Range(-1f, 1f)]
    [SerializeField] float lineHigh; // 射線高度


    // 射線偵測某個方向是否碰到有 CustomScript 的物件，並回傳其 myProperty 值
    public string DetectObjectInDirection(Vector3 direction, float distance = 1f)
    {
        distance = lineDistance;
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * lineHigh; // 避免射到地板
        if (Physics.Raycast(origin, direction, out hit, distance))
        {
            //Debug.Log("偵測到物體：" + hit.collider.name);

            // 嘗試取得 Building 類別
            Building building = hit.collider.GetComponent<Building>();
            if (building != null && building.buildingObj != null)
            {
                // 取得 ScriptableObject 的資料
                string name = building.buildingObj.name;
                string type = building.buildingObj.buildingType.ToString();
                bool block = building.buildingObj.isVisionBlocking;

                //Debug.Log($"偵測到建築：{name} | 類型：{type} | 擋視線：{block}");
                return type;  // 你可以改成 return $"{type} ({block})"; 或其他格式
            }
            else
            {
                return "未掛 Building 或未設定 BuildingData";
            }
        }
        else
        {
            return "未碰到任何物體";
        }
    }


#if UNITY_EDITOR
private void OnDrawGizmos()
{
    if (grid == null) return;

    // 起點：目前角色所在格子
    Vector3Int currentCellGizmo = grid.WorldToCell(transform.position);
    Vector3 center = grid.GetCellCenterWorld(currentCellGizmo);

    // 計算朝向方向
    Vector3Int forwardGridDir = new Vector3Int(Mathf.RoundToInt(moveVector.x), 0, Mathf.RoundToInt(moveVector.y));

    // 顏色與大小
    Gizmos.color = Color.red;
    Vector3 cellSize = new Vector3(1, 0.1f, 1); // 假設格子大小 1x1，Y壓扁避免穿透太高

    // 繪製兩個前方格子
    for (int i = 1; i <= 2; i++)
    {
        Vector3Int targetCell = currentCellGizmo + forwardGridDir * i;
        Vector3 cellCenter = grid.GetCellCenterWorld(targetCell);
        Gizmos.DrawCube(cellCenter, cellSize);
    }
}
#endif



    /// <summary>
    /// 偵測角色前方兩格內的物件，回傳物件名稱清單（每一格一組）
    /// </summary>
    //private List<List<string>> DetectObjectsInFrontTwoTilesSeparated()
    //{
    //    List<List<string>> allResults = new List<List<string>>();

    //    // 計算角色目前在的格子與方向
    //    Vector3Int forwardGridDir = new Vector3Int(Mathf.RoundToInt(moveVector.x), 0, Mathf.RoundToInt(moveVector.y));

    //    for (int i = 1; i <= 2; i++)
    //    {
    //        List<string> tileResults = new List<string>();
    //        Vector3Int checkCell = currentCell + forwardGridDir * i;
    //        Vector3 worldPos = grid.GetCellCenterWorld(checkCell);

    //        Collider[] hits = Physics.OverlapBox(worldPos, Vector3.one * 0.4f);
    //        foreach (var hit in hits)
    //        {
    //            tileResults.Add(hit.gameObject.name);
    //        }

    //        allResults.Add(tileResults);
    //    }

    //    return allResults;
    //}
    /// <summary>
    /// 偵測角色前方兩格內的 Building 物件（每格一組）
    /// </summary>
    private List<List<Building>> DetectBuildingsInFrontTwoTiles()
    {
        List<List<Building>> allResults = new List<List<Building>>();
        // ✅ 取得角色輸入方向（以 moveVector 決定格子方向）
        Vector3Int forwardGridDir = new Vector3Int(Mathf.RoundToInt(moveVector.x), 0, Mathf.RoundToInt(moveVector.y));

        // ✅ 分別偵測前方第 1 格、第 2 格
        for (int i = 1; i <= 2; i++)
        {
            List<Building> tileBuildings = new List<Building>();
            Vector3Int checkCell = currentCell + forwardGridDir * i;  // 目標格子
            Vector3 worldPos = grid.GetCellCenterWorld(checkCell);   // 轉換為世界座標

            // ✅ 檢查格子中所有碰撞體，抓取帶有 Building 的物件
            Collider[] hits = Physics.OverlapBox(worldPos, Vector3.one * 0.4f); // 使用小盒子掃描格子
            foreach (var hit in hits)
            {
                Building b = hit.GetComponent<Building>();
                if (b != null)
                    tileBuildings.Add(b); // 加入此格的 Building
            }

            allResults.Add(tileBuildings); // 加入結果
        }

        return allResults;
    }

    private Vector3Int GetGridDirectionFromFacing(Vector3 forward)
    {
        forward.y = 0;
        forward.Normalize();

        if (Vector3.Dot(forward, Vector3.forward) > 0.7f)
            return new Vector3Int(0, 0, 1);
        if (Vector3.Dot(forward, Vector3.back) > 0.7f)
            return new Vector3Int(0, 0, -1);
        if (Vector3.Dot(forward, Vector3.right) > 0.7f)
            return new Vector3Int(1, 0, 0);
        if (Vector3.Dot(forward, Vector3.left) > 0.7f)
            return new Vector3Int(-1, 0, 0);

        return Vector3Int.zero;
    }

}