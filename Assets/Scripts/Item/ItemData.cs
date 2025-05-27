using UnityEngine;
/// <summary>
/// 道具可編程物件 - mobias
/// </summary>

[CreateAssetMenu(fileName = "itemObject", menuName = "CreateItem")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public enum ItemType { None, }
    public ItemType itemType;
}
