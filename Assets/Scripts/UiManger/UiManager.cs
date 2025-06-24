using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
/// <summary>
/// 顯示道具腳本 - mobias
/// </summary>

public class UIManager : MonoBehaviour
{

    private GameObject player;
    public GameObject cardPrefab;         // 🃏 卡牌預製物
    public RectTransform cardPanel;       // 📦 放卡牌的 Panel（要拉 Panel 的 RectTransform）
    public ItemData[] weaponItems; // 在 Inspector 中拉入 ScriptableObject 陣列

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        // 設定 Panel 錨點（保持）
        cardPanel.anchorMin = new Vector2(0.5f, 0f);
        cardPanel.anchorMax = new Vector2(0.5f, 0f);
        cardPanel.pivot = new Vector2(0.5f, 0f);
        cardPanel.anchoredPosition = new Vector2(0, 0);
        cardPanel.sizeDelta = new Vector2(600, 120);

        // ✅ 依照 ScriptableObject 清單建立卡片
        foreach (var item in weaponItems)
        {
            if (item != null)
                CreateCard(item);
            else
                Debug.LogWarning($"item 是空的");
        }
    }


    public void CreateCard(ItemData itemData)
    {
        GameObject card = Instantiate(cardPrefab, cardPanel);

        // 🔒 設定 UI 名稱
        TextMeshProUGUI tmp = card.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = itemData.itemName;
        else
            Debug.LogWarning("❗ 無法找到 TextMeshProUGUI 元件，請確認 prefab 結構");

        // ✅ 嘗試使用 itemName 當類別名稱（需加上命名空間）
        ItemScript itemScript = null;

        // 假設你的類別像 Pistol 是放在 global namespace（沒有命名空間）
        // 若有，請填上正確命名空間，例如："Game.Items." + itemData.itemName;
        string fullClassName = itemData.itemName;

        // 若你有自己的命名空間，請寫成：
        // string fullClassName = "MyGame.Weapons." + itemData.itemName;

        // 🔍 取得目前 Assembly 來尋找類別（Unity 通常不會直接找到 global class）
        Type type = null;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(fullClassName);
            if (type != null)
                break;
        }

        if (type == null)
        {
            Debug.LogError($"❌ 找不到類別：{fullClassName}，請確認類別名稱與 itemName 完全一致，或是否需要補上命名空間");
            return;
        }

        // ✅ 加上腳本並初始化
        itemScript = (ItemScript)card.AddComponent(type);
        itemScript.ItemInitialize(itemData);

        // ✅ 設定拖曳控制
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


    //void showItem(player)
    //{

    //}

    // 修改 UIManager
    public void useItem(ItemScript script)
    {
        Debug.Log("🃏 使用了卡片：「" + script.itemName + "」");
        script.Use(); // ✅ 多型解法，只呼叫 Use，不管它是誰
    }
    //void attackRecord()
    //{

    //}

}
