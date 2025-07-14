using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem;

/// <summary>
/// 顯示道具腳本 - mobias
/// </summary>
public class UIManager : MonoBehaviour
{
    private GameObject player;

    public bool isCardLocking = false; // ✅ UI 鎖定狀態（鎖定操作）
    public bool isPlayerLocked = false; // ✅ 玩家是否可移動
    public ItemScript currentUsingCard;

    [Header("UI 元件")]
    public GameObject cardPrefab;             // 🃏 卡牌預製物
    public RectTransform cardPanel;           // 📦 放卡牌的 Panel

    [Header("道具設定")]
    public ItemData[] weaponItems;            // 在 Inspector 中拉入 ScriptableObject 陣列

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Item");

        SetupCardPanelLayout();
        foreach (var item in weaponItems)
        {
            if (item != null)
                CreateCard(item);
            else
                Debug.LogWarning("⚠️ item 是空的，請確認設定");
        }
    }

    // 建立一張卡片，並附加對應的腳本與初始化
    public void CreateCard(ItemData itemData)
    {
        GameObject card = Instantiate(cardPrefab, cardPanel);

        // 設定 UI 顯示名稱
        var tmp = card.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = itemData.itemName;
        else
            Debug.LogWarning("❗ 無法找到 TextMeshProUGUI 元件，請確認 prefab 結構");

        // 根據 itemData.itemName 找出對應腳本類別
        Type type = GetItemScriptType(itemData.itemName);
        if (type == null)
        {
            Debug.LogError($"❌ 找不到類別：{itemData.itemName}，請確認類名是否正確");
            return;
        }

        // 加上腳本並初始化
        ItemScript itemScript = (ItemScript)card.AddComponent(type);
        itemScript.ItemInitialize(itemData);

        // 設定 CanvasGroup
        var canvasGroup = card.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = card.AddComponent<CanvasGroup>();
        itemScript.cardCanvasGroup = canvasGroup;

        // 設定拖曳控制
        var drag = card.GetComponent<CardDragHandler>();
        if (drag != null)
        {
            drag.cardName = itemData.itemName;
            drag.UiManager = this;
            drag.attachedScript = itemScript;
        }
        else
        {
            Debug.LogWarning("⚠️ 卡片上缺少 CardDragHandler 腳本！");
        }
    }

    // 根據道具名稱尋找對應腳本類別
    private Type GetItemScriptType(string className)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(className);
            if (type != null)
                return type;
        }
        return null;
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

        var playerScript = PlayerScript.GetInstance();
        if (playerScript == null)
        {
            Debug.LogError("❌ 找不到 PlayerScript 實例");
            return;
        }

        // 根據實際類型處理，但都能指定給 currentCard（因為 currentCard 是 ItemScript）
        if (script is RangeWeapon rangeWeapon)
        {
            playerScript.RangeCurrentCard = rangeWeapon;
        }
        else if (script is ThrowWeapon throwWeapon)
        {
            playerScript.ThrowCurrentCard = throwWeapon;
        }

        // 執行 Use 行為
        script.Use();
    }

}
