using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 門的資料 - js5515
/// </summary>
[System.Serializable]
public class DoorData
{
    [SerializeField] private PrefabSpawnData psd;
    [SerializeField] private string roomName1;
    [SerializeField] private string roomName2;
    public PrefabSpawnData Psd
    {
        get => psd;
        set => psd = value;
    }
    public string RoomName1
    {
        get => roomName1;
        set => roomName1 = value;
    }
    public string RoomName2
    {
        get => roomName2;
        set => roomName2 = value;
    }

    public DoorData(PrefabSpawnData psd, string roomName1, string roomName2)
    {
        this.psd = psd;
        this.roomName1 = roomName1;
        this.roomName2 = roomName2;
    }
}
