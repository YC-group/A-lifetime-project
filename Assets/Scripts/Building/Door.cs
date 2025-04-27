using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 門的邏輯 - js5515
/// </summary>
public class Door : MonoBehaviour
{
    [SerializeField] private DoorData doorData;

    public void SetDataAndLinks(DoorData data, List<int> value)
    {
        this.doorData = data;
        this.doorData.SetLinks(value);
    }
}
