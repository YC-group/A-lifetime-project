using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Behavior;

/// <summary>
/// 回合控制腳本 - Jerry0401
/// </summary>
///

public enum RoundState
{
    PlayerTurn,
    EnemyTurn,
    EndingTurn
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private RoundState currentRound;
    [SerializeField] private int roundCounts;
    public bool SHOWROUNDSTATE = true;
    public bool SHOWENEMYSTUNSTATE = true;
    
    private static GameManager GetInstance() // Singleton
    {
        if (Instance == null)
        {
            Instance = GameObject.FindAnyObjectByType<GameManager>();
            if (Instance == null)
            {
                Debug.LogError("GameManager not found");
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
            Debug.LogWarning("Multiple GameManagers found");
            Destroy(this.gameObject);
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
            SetToNextRound();
        }
    }

    /// <summary>
    /// 遊戲開始且從玩家回合開始
    /// </summary>
    /// <returns>bool</returns>
    private bool GameStart() // 遊戲開始且從玩家回合開始
    {
        currentRound = RoundState.PlayerTurn; // 切換為玩家回合
        roundCounts = 0;
        ShowRoundState(currentRound);
        return true;
    }
    /// <summary>
    /// 回合重新開始
    /// </summary>
    /// <returns>bool</returns>
    public bool RoundReset()
    {
        currentRound = RoundState.PlayerTurn;
        roundCounts = 0;
        ShowRoundState(currentRound);
        return true;
    }
    /// <summary>
    /// 設定回合
    /// </summary>
    /// <returns>void</returns>
    public void SetCurrentRound(RoundState roundState)
    {
        this.currentRound = roundState;
        ShowRoundState(currentRound);
        roundCounts ++;
    }

    /// <summary>
    /// 獲得現在回合
    /// </summary>
    /// <returns>RoundState</returns>
    public RoundState GetCurrentRound()
    {
        return this.currentRound;
    }

    /// <summary>
    /// 切換至下個回合
    /// </summary>
    /// <returns>void</returns>
    public void SetToNextRound()
    {
        if (currentRound.Equals(RoundState.PlayerTurn))
        {
            currentRound = RoundState.EnemyTurn;
            ShowRoundState(currentRound);
        }
        else if (currentRound.Equals(RoundState.EnemyTurn))
        {
            currentRound = RoundState.EndingTurn;
            ShowRoundState(currentRound);
        }
        else if (currentRound.Equals(RoundState.EndingTurn))
        {
            currentRound = RoundState.PlayerTurn;
            ShowRoundState(currentRound);
        }
        roundCounts++;
    }

    /// <summary>
    /// 回傳經過幾個回合
    /// </summary>
    /// <returns>int</returns>
    public int GetAfterRoundsCounts()
    {
        return roundCounts;
    }
    
    /// <summary>
    /// 顯示回合在除錯介面上
    /// </summary>
    /// <returns>void</returns>
    private void ShowRoundState(RoundState roundState)
    {
        if (SHOWROUNDSTATE)
        {
            Debug.Log("Round : " + roundState);
        }
    }
    
}