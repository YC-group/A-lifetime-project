using UnityEngine;
/// <summary>
/// ¼Ä¤Hª«¥ó - mobias
/// </summary>

[CreateAssetMenu(fileName = "enemyObject", menuName = "CreateEnemy")]
public class EnemyData : ScriptableObject
{
    public string name;
    public float hp;
    public float attack;
    //public Item weapon;
    public enum SpecialAct { None, }
    public SpecialAct specialAct;
    public float speed;
}

