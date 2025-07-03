using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 物品共通屬性腳本 - Jerry0401
/// </summary>
public abstract class ItemScript : MonoBehaviour
{
    [SerializeField] protected ItemType itemType;
    [SerializeField] public string itemName;
    [SerializeField] protected string itemDescription;
    [SerializeField] protected int damage;
    [SerializeField] protected float range;
    
    public ItemData itemSO;
    public CanvasGroup cardCanvasGroup; // 所有子類可共用此 CanvasGroup（用來控制 UI 顯示）

    public virtual void AddItemToPocket() // 將物品加入口袋
    {
        PlayerScript.Instance.pocketList.Add(this);
        Debug.Log("Pocket Counts: " + PlayerScript.Instance.pocketList.Count);
        Destroy(this.gameObject);
    }

    public virtual void RemoveItemFromPocket() // 將物品從口袋刪除
    {
        PlayerScript.Instance.pocketList.Remove(this);
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

        Debug.Log("✅ 初始化完成：" + itemName);
    }


    public abstract void Attack(); // 讓子物件實作攻擊
    
    private void OnTriggerEnter(Collider other)
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
    }
}
