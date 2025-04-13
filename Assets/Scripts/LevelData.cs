using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���d��� - js5515
/// </summary>
[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public List<PrefabSpawnData> barriers;
    public List<RoomData> rooms;
}
