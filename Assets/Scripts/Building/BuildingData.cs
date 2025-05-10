using UnityEngine;
/// <summary>
/// ���ݩ� - mobias
/// </summary>


[CreateAssetMenu(fileName = "BuildingObject", menuName = "Game/Building Data")]
public class BuildingData : ScriptableObject
{
    // ����W��
    public string BulidingName;

    // ��������]Hardwall, Softwall, Mechanism�^
    public enum BuildingType { None, Hardwall, Softwall, Mechanism }
    public BuildingType buildingType;

    // �O�_���׵��u
    public bool isVisionBlocking;

    //�O�_�i��V
    public bool isCrossable;

}
