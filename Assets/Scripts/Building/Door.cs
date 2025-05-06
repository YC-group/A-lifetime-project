using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 門的邏輯 - js5515
/// </summary>
public class Door : MonoBehaviour
{
    [SerializeField] private string roomName1;
    [SerializeField] private string roomName2;

    public void SetRoomName(DoorData doorData)
    {
        this.roomName1 = doorData.RoomName1;
        this.roomName2 = doorData.RoomName2;
    }
}
