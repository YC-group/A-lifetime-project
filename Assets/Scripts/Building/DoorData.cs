using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 門的資料 - js5515
/// </summary>
[CreateAssetMenu(fileName = "DoorData", menuName = "Game/Door Data")]
public class DoorData : ScriptableObject
{
    [SerializeField] private List<int> links;

    public void SetLinks(List<int> value)
    {
        this.links = value;
    }
}
