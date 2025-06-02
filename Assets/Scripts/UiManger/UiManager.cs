using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// 顯示道具腳本 - mobias
/// </summary>

public class UiManager : MonoBehaviour
{

    //private GameObject player;
    public GameObject cardPrefab;         // 🃏 卡牌預製物
    public RectTransform cardPanel;       // 📦 放卡牌的 Panel（要拉 Panel 的 RectTransform）

    void Start()
    {
        // 🎯 設定 CardPanel 在畫面下方中間（錨點與位置）
        cardPanel.anchorMin = new Vector2(0.5f, 0f);
        cardPanel.anchorMax = new Vector2(0.5f, 0f);
        cardPanel.pivot = new Vector2(0.5f, 0f);
        cardPanel.anchoredPosition = new Vector2(0, 0); // 距離底部 20 單位
        cardPanel.sizeDelta = new Vector2(600, 120);     // 寬度、高度

        // 🃏 動態產生卡牌
        ShowCardUI();
    }

    void ShowCardUI()
    {
        for (int i = 0; i < 3; i++) // 產生 5 張卡牌
        {
            GameObject card = Instantiate(cardPrefab, cardPanel);
            card.GetComponentInChildren<TextMeshProUGUI>().text = "CARD " + (i + 1);

        }
    }

    //void showItem(player)
    //{

    //}

    //void useItem(player, enemy)
    //{

    //}

    //void attackRecord()
    //{

    //}

}
