using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 顯示道具腳本 - mobias
/// </summary>

public class UiManager : MonoBehaviour
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


    //public void ShowCardUI()
    //{
    //    string[] cardNames = { "gun", "Melee", "Throw" }; // 可以來自資料庫或 ScriptableObject

    //    for (int i = 0; i < cardNames.Length; i++)
    //    {
    //        GameObject card = Instantiate(cardPrefab, cardPanel);
    //        card.GetComponentInChildren<TextMeshProUGUI>().text = cardNames[i];

    //        var dragScript = card.GetComponent<CardDragHandler>();
    //        dragScript.player = player;
    //        dragScript.uiManager = this;
    //        dragScript.cardName = cardNames[i]; // ✅ 設定卡片名稱
    //    }
    //}

    public void CreateCard(ItemData itemData)
    {
        GameObject card = Instantiate(cardPrefab, cardPanel);

        // 🔒 安全設置文字
        TextMeshProUGUI tmp = card.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = itemData.itemName;
        }
        else
        {
            Debug.LogError("❌ 無法找到 TextMeshProUGUI，請檢查 prefab 結構並套用 apply");
        }

        // ✅ 安全加上武器腳本
        if (itemData.itemType == ItemType.RangeWeapon)
        {
            var range = card.AddComponent<RangeWeapon>();
            if (range != null)
            {
                range.weaponSO = itemData;
            }
            else
            {
                Debug.LogError("❌ RangeWeapon 腳本加載失敗，請確認不是 abstract 類別！");
            }
        }

        // ✅ 卡片拖曳
        var drag = card.GetComponent<CardDragHandler>();
        if (drag != null)
        {
            drag.cardName = itemData.itemName;
            drag.uiManager = this;
        }
    }



    //void showItem(player)
    //{

    //}

    public void useItem(string cardName)
    {
        Debug.Log("🃏 玩家 " + player.name + " 使用了卡片：「" + cardName + "」");
    }
    //void attackRecord()
    //{

    //}

}
