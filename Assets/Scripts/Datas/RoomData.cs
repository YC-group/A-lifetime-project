using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 房間資料 - js5515
/// </summary>
[CreateAssetMenu(fileName = "RoomData", menuName = "Game/Room Data")]
public class RoomData : ScriptableObject
{
    [SerializeField] private List<PrefabSpawnData> enemies;
    [SerializeField] private List<PrefabSpawnData> items;
    [SerializeField] private List<PrefabSpawnData> buildings;
    [SerializeField] private Vector3 spawnpoint;

    public List<PrefabSpawnData> Enemies
    {
        get => enemies;
        set => enemies = value;
    }
    public List<PrefabSpawnData> Items
    {
        get => items;
        set => items = value;
    }
    public List<PrefabSpawnData> Buildings
    {
        get => buildings;
        set => buildings = value;
    }
    public Vector3 Spawnpoint
    {
        get => spawnpoint;
        set => spawnpoint = value;
    }
}
