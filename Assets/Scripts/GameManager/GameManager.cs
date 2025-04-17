using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 回合控制腳本 - Jerry0401
/// </summary>
public enum RoundState // 回合列舉
{
    PlayerTurn,
    EnemyTurn,
    EndingTurn
}
public class GameManager : MonoBehaviour
{
    public static GameManager GameState;
    private RoundState currentRound;

    private void Awake()
    {
        GameState = this; // 實例化 GameManager 確保全局都能使用
        // throw new NotImplementedException();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameStart(); // 遊戲開始
    }

    // Update is called once per frame
    void Update()
    {
        // 測試回合用 按下並放開 R 可切換到下回合
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (currentRound.Equals(RoundState.PlayerTurn))
            {
                currentRound = RoundState.EnemyTurn;
                Debug.Log(currentRound);
            }
            else if (currentRound.Equals(RoundState.EnemyTurn))
            {
                currentRound = RoundState.EndingTurn;
                Debug.Log(currentRound);
            }
            else if (currentRound.Equals(RoundState.EndingTurn))
            {
                currentRound = RoundState.PlayerTurn;
                Debug.Log(currentRound);
            }
        }
    }

    private bool GameStart()
    {
        currentRound = RoundState.PlayerTurn; // 切換為玩家回合
        Debug.Log(currentRound);
        return true;
    }

    public void SetCurrentRound(RoundState roundState)
    {
        this.currentRound = roundState;
        Debug.Log(currentRound);
    }

    public RoundState GetCurrentRound()
    {
        return this.currentRound;
    }
}