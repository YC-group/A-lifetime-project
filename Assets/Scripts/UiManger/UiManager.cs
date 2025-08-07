using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem;
using System.Collections.Generic;
/// <summary>
/// 顯示道具腳本 - mobias
/// </summary>
public class UIManager : MonoBehaviour
{
    private PlayerScript playerScript;

    public bool isCardLocking = false; // ✅ UI 鎖定狀態（鎖定操作）
    public bool isPlayerLocked = false; // ✅ 玩家是否可移動
    public ItemScript currentUsingCard;
    public List<GameObject> pocketList; // 儲存item GO

    [Header("UI 元件")]
    public GameObject cardPrefab;             // 🃏 卡牌預製物
    public RectTransform cardPanel;           // 📦 放卡牌的 Panel

    void Start()
    {
        playerScript = PlayerScript.GetInstance();
        pocketList = playerScript.pocketList;

        SetupCardPanelLayout();
        foreach (var go in pocketList)
        {
            var script = go.GetComponent<ItemScript>();
            CreateCard(script);
        }
    }

    // 建立一張卡片，並附加對應的腳本與初始化
    public void CreateCard(ItemScript itemScript)
    {
        GameObject card = Instantiate(cardPrefab, cardPanel);

        var attachedScript = (ItemScript)card.AddComponent(itemScript.GetType());

        attachedScript.ItemInitialize(itemScript.itemSO);
        attachedScript.attachedCardUI = card; // ✅ 綁定回 UI 卡片

        //給予bulletCount ⚠待優化
        if (itemScript is RangeWeapon originalWeapon && attachedScript is RangeWeapon newWeapon)
        {
            newWeapon.bulletCount = originalWeapon.bulletCount;
        }


        var tmp = card.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = itemScript.itemName;

        var drag = card.GetComponent<CardDragHandler>();
        if (drag != null)
        {
            drag.UiManager = this;
            drag.attachedScript = attachedScript;
        }


    }

    // 設定卡片面板位置與大小
    private void SetupCardPanelLayout()
    {
        cardPanel.anchorMin = new Vector2(0.5f, 0f);
        cardPanel.anchorMax = new Vector2(0.5f, 0f);
        cardPanel.pivot = new Vector2(0.5f, 0f);
        cardPanel.anchoredPosition = Vector2.zero;
        cardPanel.sizeDelta = new Vector2(600, 120);
    }

    // 呼叫道具的 Use() 行為
    public void useItem(ItemScript script)
    {
        currentUsingCard = script;
        scriptToPlayer(script); 
        script.Use();// 執行 Use 行為
    }

    public void throwItem(ItemScript script)
    {
        currentUsingCard = script;
        scriptToPlayer(script);
        script.Throw();
    }

    // 根據實際類型處理，但都能指定給 currentCard（因為 currentCard 是 ItemScript）
    private void scriptToPlayer(ItemScript script)
    {
        playerScript.CurrentCard = script;
        if (script is RangeWeapon rangeWeapon)
        {
            playerScript.RangeCurrentCard = rangeWeapon;
        }
    }
}
