using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
/// <summary>
/// 物品共通屬性腳本 - Jerry0401
/// </summary>
public abstract class ItemScript : MonoBehaviour
{
    [SerializeField] public string itemName;
    [SerializeField] public ItemType itemType;
    [SerializeField] protected string itemDescription;
    [SerializeField] protected int damage;
    [SerializeField] protected float range;

    public PlayerScript playerScript;
    public ItemData itemSO;
    public GameObject cardPrefab; // ✅ 每張道具對應的 prefab
    public GameObject attachedCardUI; // ✅ 對應的 UI 卡牌
    public bool throwSelectEnemy = false;
    public bool Selected = false;
    private Transform Target;
  

    public virtual void Awake()
    {
        playerScript = PlayerScript.GetInstance();

    }

    public virtual void AddItemToPocket() // 將物品加入口袋
    {
        playerScript.pocketList.Add(this.gameObject);
        Debug.Log("Pocket Counts: " + playerScript.pocketList.Count);
        Destroy(this.gameObject);
    }

    public virtual void RemoveItemFromPocket() // 將物品從口袋刪除
    {
        playerScript.pocketList.Remove(this.gameObject);
        // ✅ 通知 UI 重新排列
        attachedCardUI.SetActive(false); // ✅ 先隱藏，才能觸發排版更新

        var layoutGroup = attachedCardUI.transform.parent as RectTransform;
        if (layoutGroup != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup); // ✅ 重新排版
        }
    }

    public virtual void DropItem() // 掉落物品
    {
        
    }
    public virtual void ItemInitialize(ItemData itemSO)
    {
        this.itemSO = itemSO;

        itemType = itemSO.itemType;
        itemName = itemSO.itemName;
        itemDescription = itemSO.itemDescription;
        damage = itemSO.damage;
        range = itemSO.range;

    }
    public virtual void Attack()
    {

    }

    private void OnTriggerEnter(Collider other) // 撿起道具
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log("OnTriggerEnter");
            AddItemToPocket();
        }
    }

    public virtual void Use()
    {
        Debug.Log($"🧪 使用了通用道具：{itemName}");
        Selected =true;
        playerScript.isCardDragging = true;
    }

    public virtual void CancelAttackAndRestore()
    {
        Debug.Log("❌ 攻擊取消");
        playerScript.isCardDragging = false;
        Selected = false;
        var dragHandler = GetComponent<CardDragHandler>();
        if (dragHandler != null)
        {

            dragHandler.ResetUsedFlag();       // ✅ 重設 used
            dragHandler.RestoreDisplay();      // ✅ 改為完整重顯 UI
        }
    }

    public virtual void Throw()
    {
        playerScript.isCardDragging = true;
        throwSelectEnemy = true;
    }

    public void SelectThrowTarget()
    {
        if (!throwSelectEnemy) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Enemy"))
        {
            Target = hit.collider.transform;
            ThrowOut();
        }
    }

    public virtual void ThrowOut()
    {

        Debug.Log($"➡ 丟擲敵人：{Target.name}");
        throwSelectEnemy = false;
        playerScript.isCardDragging = false;
        RemoveItemFromPocket();

    }


}
