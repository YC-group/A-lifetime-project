using UnityEngine;
/// <summary>
/// 關卡生成以及房間管理 - js5515
/// </summary>
public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    private string currentRoomName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void ChangeRoom()
    {

    }
}
