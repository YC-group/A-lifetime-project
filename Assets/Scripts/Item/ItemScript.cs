using UnityEngine;
/// <summary>
/// 物品共通屬性腳本 - Jerry0401
/// </summary>
public abstract class ItemScript : MonoBehaviour
{
    public ItemType itemType;
    public string itemName;
    public string itemDescription;
    public int damage;
    public float range;
    public virtual void AddItemToPocket() // 將物品加入口袋
    {
        
    }

    public virtual void RemoveItemFromPocket() // 將物品從口袋刪除
    {
        
    }

    public virtual void DropItem() // 掉落物品
    {
        
    }

    public virtual void ItemInitailize(ItemData itemSO) // 初始化
    {
        itemType = itemSO.itemType;
        itemName = itemSO.name;
        itemDescription = itemSO.itemDescription;
        damage = itemSO.damage;
        range = itemSO.range;
    }
    public abstract void Attack(); // 讓子物件實作攻擊
}
