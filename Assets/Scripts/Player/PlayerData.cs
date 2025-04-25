using UnityEngine;
/// <summary>
/// 角色可編程物件 - Jerry0401
/// </summary>

[CreateAssetMenu(fileName = "playerObject", menuName = "CreatePlayer")]
public class PlayerData : ScriptableObject
{
    public string playerName;
    public float attack;
    public float hp;
    public int moveDistance; // 每步移動距離
    public float moveTime; // 每步移動時間
}
