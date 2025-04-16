using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// 角色移動腳本 - Jerry0401
/// </summary>

public class PlayerScript : MonoBehaviour
{
    
    [SerializeField] public PlayerScriptableObject playerObj; // 序列化玩家物件
    private bool isMoving = false; // 判斷玩家是否正在移動
    private Rigidbody rb;
    private Vector2 moveVector;

    private void Start()
    {
        rb = this.transform.GetComponent<Rigidbody>();
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
                Vector3 direction = new Vector3(moveVector.x, 0, moveVector.y).normalized;
                Vector3 destination = transform.position + direction * playerObj.moveDistance; // 計算目的地

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
        GameManager.GameState.SetCurrentRound(RoundState.EnemyTurn); // 敵人回合開始
    }
}
