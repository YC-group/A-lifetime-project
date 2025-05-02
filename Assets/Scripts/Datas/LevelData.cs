using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 關卡資料 - js5515
/// </summary>
[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [SerializeField] private List<PrefabSpawnData> barriers;
    [SerializeField] private List<RoomData> rooms;
    [SerializeField] private List<DoorData> doors;
    [SerializeField] private RoomData startRoomData;

    public List<PrefabSpawnData> Barriers
    {
        get => barriers;
        set => barriers = value;
    }
    public List<RoomData> Rooms
    {
        get => rooms;
        set => rooms = value;
    }
    public List<DoorData> Doors
    {
        get => doors;
        set => doors = value;
    }
    public RoomData StartRoomData
    {
        get => startRoomData;
        set => startRoomData = value;
    }
}
