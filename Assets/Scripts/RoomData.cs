using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �ж���� - js5515
/// </summary>
[CreateAssetMenu(fileName = "RoomData", menuName = "Game/Room Data")]
public class RoomData : ScriptableObject
{
    public List<PrefabSpawnData> enemies;
    public List<PrefabSpawnData> items;
    public List<PrefabSpawnData> buildings;
    public List<RoomData> linkRooms;
}
