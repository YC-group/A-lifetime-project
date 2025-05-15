using UnityEngine;
/// <summary>
/// ���ݩ� - mobias
/// </summary>


[CreateAssetMenu(fileName = "BuildingObject", menuName = "Game/Building Data")]
public class BuildingData : ScriptableObject
{

    public string BulidingName;    // ����W��
    public enum BuildingType { None, Hardwall, Softwall, Mechanism }     // ��������]Hardwall, Softwall, Mechanism�^
    public BuildingType buildingType;
    public bool isVisionBlocking;  // �O�_���׵��u
    public bool isCrossable;    //�O�_�i��V

}
