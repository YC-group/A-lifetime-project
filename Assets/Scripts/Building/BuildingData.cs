using UnityEngine;
/// <summary>
/// 牆的屬性 - mobias
/// </summary>


[CreateAssetMenu(fileName = "BuildingObject", menuName = "Game/Building Data")]
public class BuildingData : ScriptableObject
{

    public string BulidingName;    // 牆壁名稱
    public enum BuildingType { None, Hardwall, Softwall, Mechanism }     // 牆壁類型（Hardwall, Softwall, Mechanism）
    public BuildingType buildingType;
    public bool isVisionBlocking;  // 是否阻擋視線
    public bool isCrossable;    //是否可跨越

}
