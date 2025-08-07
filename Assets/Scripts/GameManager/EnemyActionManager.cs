using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敵人行動廣播 - Jerry0401
/// </summary>
///
public class EnemyActionManager : MonoBehaviour
{
    private static EnemyActionManager Instance;
    
    [SerializeField] private bool isScheduling;
    
    public GameObject[] EnemyGameObjects;
    
    public static EnemyActionManager GetInstance() // Singleton
    {
        if (Instance == null)
        {
            Instance = GameObject.FindAnyObjectByType<EnemyActionManager>();
            if (Instance == null)
            {
                Debug.LogError("EnemyActionManager not found");
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
            Debug.LogWarning("Multiple EnemyActionManager found");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        EnemyGameObjects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in EnemyGameObjects)
        {
            GridManager.GetInstance().AddGameObjectToMoveGrid(enemy); // 將敵人物件存入網格陣列
        }
        StartCoroutine(EnemyMoveSchedule());
    }

    private void OnDisable()
    {
        EnemyGameObjects = null;
        isScheduling = false;
    }
    
    // HACK: 敵人移動規劃
    // 之後會把 FindGameObjectsWithTag 改掉
    // 因為不適用於每個房間
    public IEnumerator EnemyMoveSchedule()
    {
        if (EnemyGameObjects == null)
        {
            Debug.LogWarning("Can't found enemy !");
        }

        GameObject[] sortGameObjects =
            EnemyGameObjects.OrderByDescending(go => go.GetComponent<EnemyScript>().movePriority)
                .ToArray(); // 將敵人按照 MovePriority 降冪排序

        int sortGameObjectIndex = 0;
        while (sortGameObjectIndex < sortGameObjects.Length)
        {
            var enemy = sortGameObjects[sortGameObjectIndex].GetComponent<EnemyScript>();
            enemy.isSelected = true;
            // HACK: 應該在 EnemyScript 初始化 State
            if (enemy.isStun == false) // 在沒有被擊暈的狀態下
            {
                if (enemy.isAlert)
                {
                    enemy.stateMachine.ChangeState(enemy.navigateState);
                }
                enemy.stateMachine.currentState.Update();

                while (enemy.isMoving)
                {
                    yield return null;
                }
            }
            sortGameObjectIndex++;
            enemy.isSelected = false;
        }
        GameManager.GetInstance().SetToNextRound();
    }
}