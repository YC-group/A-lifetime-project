using UnityEngine;
/// <summary>
/// 敵人物件 - mobias
/// </summary>

[CreateAssetMenu(fileName = "enemyObject", menuName = "CreateEnemy")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float hp;
    public float attack;
    //public Item weapon;
    public enum SpecialAct { None, }
    public SpecialAct specialAct;
    public float speed;
}

