using System;
using UnityEngine;
/// <summary>
/// 重生點資料(JSON) - js5515
/// </summary>
[Serializable]
public class SpawnSave
{
    [SerializeField] private Vector3 spawnpoint;
    [SerializeField] private string roomId;

    public Vector3 Spawnpoint
    {
        get => spawnpoint;
        set => spawnpoint = value;
    }
    public string RoomId
    {
        get => roomId;
        set => roomId = value;
    }
    public SpawnSave()
    {

    }
}
