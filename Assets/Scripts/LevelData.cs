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

    public void SetBarriers(List<PrefabSpawnData> barriers)
    {
        this.barriers = barriers;
    }
}
