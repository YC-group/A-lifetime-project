using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 物品共通屬性腳本 - Jerry0401
/// </summary>
public abstract class ItemScript : MonoBehaviour
{
    private GameObject player;
    [SerializeField] protected ItemType itemType;
    [SerializeField] protected string itemName;
    [SerializeField] protected string itemDescription;
    [SerializeField] protected int damage;
    [SerializeField] protected float range;
    
    public virtual void AddItemToPocket() // 將物品加入口袋
    {
        player.GetComponent<PlayerScript>().PocketList.Add(this);
        Debug.Log("Pocket Counts: " + player.GetComponent<PlayerScript>().PocketList.Count);
        Destroy(this.gameObject);
    }

    public virtual void RemoveItemFromPocket() // 將物品從口袋刪除
    {
        player.GetComponent<PlayerScript>().PocketList.Remove(this);
    }

    public virtual void DropItem() // 掉落物品
    {
        
    }

    public virtual void ItemInitialize(ItemData itemSO) // 初始化
    {
        player = GameObject.FindGameObjectWithTag("Player");
        itemType = itemSO.itemType;
        itemName = itemSO.itemName;
        itemDescription = itemSO.itemDescription;
        damage = itemSO.damage;
        range = itemSO.range;
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
}
