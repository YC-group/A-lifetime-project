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

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        // 🎯 設定 CardPanel 在畫面下方中間（錨點與位置）
        cardPanel.anchorMin = new Vector2(0.5f, 0f);
        cardPanel.anchorMax = new Vector2(0.5f, 0f);
        cardPanel.pivot = new Vector2(0.5f, 0f);
        cardPanel.anchoredPosition = new Vector2(0, 0); // 距離底部 20 單位
        cardPanel.sizeDelta = new Vector2(600, 120);     // 寬度、高度

        // 🃏 動態產生卡牌
        ShowCardUI();
    }

    public void ShowCardUI()
    {
        string[] cardNames = { "gun", "Melee", "Throw" }; // 可以來自資料庫或 ScriptableObject

        for (int i = 0; i < cardNames.Length; i++)
        {
            GameObject card = Instantiate(cardPrefab, cardPanel);
            card.GetComponentInChildren<TextMeshProUGUI>().text = cardNames[i];

            var dragScript = card.GetComponent<CardDragHandler>();
            dragScript.player = player;
            dragScript.uiManager = this;
            dragScript.cardName = cardNames[i]; // ✅ 設定卡片名稱
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
