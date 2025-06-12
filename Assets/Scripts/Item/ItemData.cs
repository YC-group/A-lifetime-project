using System;using UnityEngine;
/// <summary>
/// 道具可編程物件 - mobias
/// </summary>

public enum ItemType{
    MeleeWeapon,
    RangeWeapon,
    ThrowWeapon
}

[CreateAssetMenu(fileName = "itemObject", menuName = "CreateItem")]
public class ItemData : ScriptableObject
{
    public ItemType itemType;
    public string itemName;
    public string itemDescription;
    public int damage;
    public float range;
    public int bulletCount;
}
