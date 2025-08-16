using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPos;
    private Canvas canvas;
    private bool used = false;

    public UIManager UiManager;
    public ItemScript attachedScript;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (!TryGetComponent(out canvasGroup))
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (UiManager == null)
        {
            UiManager = FindFirstObjectByType<UIManager>();
            if (UiManager == null) Debug.LogWarning("⚠ 無法自動找到 UiManager！");
        }
    }

    // ✅ 點一下就使用
    public void OnPointerClick(PointerEventData eventData)
    {
        if (used) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        used = true;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        UiManager.useItem(attachedScript);
    }

    // 以下保留你的拖曳/回位（若日後要全部移除也可以）
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPos = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (used || rectTransform == null || canvas == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        if (originalPos != Vector2.zero &&
            rectTransform.anchoredPosition.y > originalPos.y + 150f)
        {
            used = true;
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            UiManager.throwItem(attachedScript);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!used) StartCoroutine(SmoothReturn());
        canvasGroup.blocksRaycasts = true;
    }

    private System.Collections.IEnumerator SmoothReturn()
    {
        Vector2 start = rectTransform.anchoredPosition;
        float elapsed = 0f;
        const float duration = 0.2f;
        while (elapsed < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(start, originalPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = originalPos;
    }

    public void RestoreDisplay()
    {
        if (!canvasGroup) return;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        used = false;
    }

    public void ResetUsedFlag()
    {
        // 與舊版相容：重設使用狀態並恢復顯示
        RestoreDisplay();
    }
}
