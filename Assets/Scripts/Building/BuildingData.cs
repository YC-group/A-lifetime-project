using UnityEngine;
/// <summary>
/// 牆的屬性 - mobias
/// </summary>


[CreateAssetMenu(fileName = "BuildingObject", menuName = "Game/Building Data")]
public class BuildingData : ScriptableObject
{
    // 牆壁名稱
    public string BulidingName;

    // 牆壁類型（Hardwall, Softwall, Mechanism）
    public enum BuildingType { None, Hardwall, Softwall, Mechanism }
    public BuildingType buildingType;

    // 是否阻擋視線
    public bool isVisionBlocking;

    //是否可跨越
    public bool isCrossable;

}
