using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 關卡資料 - js5515
/// </summary>
[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public List<PrefabSpawnData> barriers;
    public List<RoomData> rooms;
}
