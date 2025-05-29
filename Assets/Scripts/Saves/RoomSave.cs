using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 房間資料(JSON) - js5515
/// </summary>
[Serializable]
public class RoomSave
{
    [SerializeField] private List<PrefabSpawnSave> enemies;
    [SerializeField] private List<PrefabSpawnSave> items;
    [SerializeField] private List<PrefabSpawnSave> buildings;
    [SerializeField] private string roomId;

    public List<PrefabSpawnSave> Enemies
    {
        get => enemies;
        set => enemies = value;
    }
    public List<PrefabSpawnSave> Items
    {
        get => items;
        set => items = value;
    }
    public List<PrefabSpawnSave> Buildings
    {
        get => buildings;
        set => buildings = value;
    }
    public string RoomId
    {
        get => roomId;
        set => roomId = value;
    }
}
