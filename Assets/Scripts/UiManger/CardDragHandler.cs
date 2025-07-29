using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// 此腳本負責處理卡片拖曳與使用的邏輯
public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string cardName; // 卡片名稱，例如「火球」、「補血藥水」

    private RectTransform rectTransform; // 卡片的 UI 元件，用來控制位置
    private CanvasGroup canvasGroup;     // 控制透明度與是否可以被點擊、擋住射線
    private Vector2 originalPos;         // 拖曳前的原始位置
    private Canvas canvas;               // 卡片所屬的 Canvas（用於處理解析度比例）
    private bool used = false;           // 是否已使用，避免重複觸發

    public GameObject player;            // 角色物件（預留給後續功能使用）
    public UIManager UiManager;          // UI 管理器，用來通知卡片被使用
    public ItemScript attachedScript;

    // 初始化元件
    void Awake()
    {

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        // 自動抓 player
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null) player = foundPlayer;
            else Debug.LogWarning("⚠ 無法自動找到 Player！");
        }

        // 自動抓 UiManager
        if (UiManager == null)
        {
            UiManager = FindFirstObjectByType<UIManager>();
            if (UiManager == null) Debug.LogWarning("⚠ 無法自動找到 UiManager！");
        }

        if (!TryGetComponent<CanvasGroup>(out canvasGroup))
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }


    // 開始拖曳時呼叫
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPos = rectTransform.anchoredPosition; // 記錄原始位置
        canvasGroup.blocksRaycasts = false; // 關閉 Raycast 擋住判定（讓丟到 Drop 區可以偵測）
    }

    // 拖曳過程中持續呼叫
    public void OnDrag(PointerEventData eventData)
    {

        // 如果已經使用過或物件失效就不處理
        if (used || rectTransform == null || canvas == null) return;

        // 根據滑鼠移動更新卡片位置（除以 scaleFactor 以符合 Canvas 縮放）
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        // 如果原始位置尚未記錄就不進行判斷
        if (originalPos == Vector2.zero) return;

        // 如果卡片往上拖曳超過 150 單位，視為使用該卡片
        if (rectTransform.anchoredPosition.y > originalPos.y + 150f)
        {
            used = true; // 標記為已使用
            //隱藏卡片
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            UiManager.useItem(attachedScript); // 通知 UI Manager 使用該卡片
        }
    }

    // 拖曳結束時呼叫
    public void OnEndDrag(PointerEventData eventData)
    {
        // 如果卡片未使用，就播放回原位動畫
        StartCoroutine(SmoothReturn());
        canvasGroup.blocksRaycasts = true; // 恢復 Raycast 判定

    }

    // 卡片平滑移動回原位的協程
    private IEnumerator SmoothReturn()
    {
        Vector2 start = rectTransform.anchoredPosition; // 當前位置
        float elapsed = 0f;
        float duration = 0.2f; // 回復花費時間

        // 線性插值回原位置
        while (elapsed < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(start, originalPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = originalPos; // 確保最後位置正確
    }
    public void ResetUsedFlag()
    {
        used = false;
    }
    public void RestoreDisplay()
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        used = false;
    }
}
