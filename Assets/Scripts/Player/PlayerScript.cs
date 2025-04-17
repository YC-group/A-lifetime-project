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
}
