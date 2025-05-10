using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// 建築邏輯 : mobias
/// </summary>

public class Building : MonoBehaviour
{
    //// 引用 ScriptableObject 資源

    public BuildingData buildingObj;

    //// 用於顯示牆壁屬性
    private void Start()
    {

        print("Building Name: " + buildingObj.name);
        Debug.Log("Building Type: " + buildingObj.buildingType);
        Debug.Log("Vision Blockage: " + buildingObj.isVisionBlocking);
    }

    //private void Start()
    //{
    //    Debug.Log("123")
    //}


    /// 執行互動邏輯（可從其他系統呼叫）
    //public void Interact()
    //{
    //    if (!isInteractable) return;

    //    Debug.Log($"{buildingName} 被互動了！");
    //    // TODO：在這裡觸發動畫、開門、開關機制等等
    //}


    ///// 接收攻擊（無血量，一擊摧毀）
    //public void TakeDamage()
    //{
    //    if (!isDestructible) return;

    //    Debug.Log($"{buildingName} 被攻擊，立即摧毀！");
    //    DestroyBuilding();


    //}


    ///// 執行破壞流程
    //private void DestroyBuilding()
    //{
    //    Debug.Log($"{buildingName} 被摧毀了！");
    //    // TODO：加入破壞動畫、粒子效果等
    //    Destroy(gameObject);
    //}
}
