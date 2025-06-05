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
    [Tooltip("Speed in NavMeshAgent , units / sec")]
    public float speed; // unit / second
    [Tooltip("Acceleration in NavMeshAgent , units^2 / sec")]
    public float acceleration; // unit^2 / second
    [Tooltip("Angular speed in NavMeshAgent , deg / sec")]
    public float angularSpeed; // deg / sec
    public int movePriority;
}

