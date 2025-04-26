using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DoorData", menuName = "Game/Door Data")]
public class DoorData : ScriptableObject
{
    [SerializeField] private List<int> links;

    public void SetLinks(List<int> value)
    {
        this.links = value;
    }
}
