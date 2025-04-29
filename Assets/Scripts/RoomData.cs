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
}
