using System;
using UnityEngine;
/// <summary>
/// 重生點資料 - js5515
/// </summary>
[Serializable]
public class SpawnData
{
    [SerializeField] private Vector3 spawnpoint;
    [SerializeField] private RoomData roomData;

    public Vector3 Spawnpoint
    {
        get => spawnpoint;
        set => spawnpoint = value;
    }
    public RoomData RoomData
    {
        get => roomData;
        set => roomData = value;
    }

    public SpawnData()
    {

    }
}
