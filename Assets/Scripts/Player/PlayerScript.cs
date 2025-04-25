using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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
        if (ctx.performed && !isMoving && GameManager.GameState.GetCurrentRound().Equals(RoundState.PlayerTurn)) // 按下 WASD 且並非正在移動
        {
            string result = DetectObjectInDirection(transform.forward, 1f);
            Debug.Log("偵測結果：" + result);

            moveVector = ctx.ReadValue<Vector2>();
            // print(moveVector);
            if (moveVector != Vector2.zero)
            {
                Vector3Int direction = new Vector3Int(Mathf.RoundToInt(moveVector.x), 0, Mathf.RoundToInt(moveVector.y)); 
                currentCell += direction * playerObj.moveDistance;
                Vector3 destination = grid.GetCellCenterWorld(currentCell);
                StartCoroutine(SmoothMove(destination)); // 開始移動動畫
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
            Debug.Log("偵測到物體：" + hit.collider.name);

            // 嘗試取得 Building 類別
            Building building = hit.collider.GetComponent<Building>();
            if (building != null && building.buildingObj != null)
            {
                // 取得 ScriptableObject 的資料
                string name = building.buildingObj.name;
                string type = building.buildingObj.buildingType.ToString();
                bool block = building.buildingObj.visionBlockage;

                Debug.Log($"偵測到建築：{name} | 類型：{type} | 擋視線：{block}");
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
        if (!Application.isPlaying && grid != null)
        {
            Gizmos.color = Color.green;

            // 設定起點（角色中心略上）
            Vector3 origin = transform.position + Vector3.up * lineHigh ;

            // 偵測方向 - 你可以改成你想要的方向（如 transform.forward）
            Vector3 direction = transform.forward;  // 這裡你也可用其他向量

            float detectDistance = lineDistance; // 偵測距離，可調

            // 畫出輔助線
            Gizmos.DrawRay(origin, direction * detectDistance);

            // 可選：在末端畫個球，當作目標點提示
            Gizmos.DrawSphere(origin + direction * detectDistance, 0.1f);
        }
    }
#endif

}
