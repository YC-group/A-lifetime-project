using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 關卡資料(JSON) - js5515
/// </summary>
[Serializable]
public class LevelSave
{
    [SerializeField] private List<PrefabSpawnSave> barriers;
    [SerializeField] private List<RoomSave> rooms;
    [SerializeField] private List<DoorSave> doors;
    [SerializeField] private string startRoomId;
    [SerializeField] private Vector3 startSpawnpoint;

    public List<PrefabSpawnSave> Barriers
    {
        get => barriers;
        set => barriers = value;
    }
    public List<RoomSave> Rooms
    {
        get => rooms;
        set => rooms = value;
    }
    public List<DoorSave> Doors
    {
        get => doors;
        set => doors = value;
    }
    public string StartRoomId
    {
        get => startRoomId;
        set => startRoomId = value;
    }
    public Vector3 StartSpawnpoint
    {
        get => startSpawnpoint;
        set => startSpawnpoint = value;
    }
}
