using UnityEngine;
/// <summary>
/// 建築邏輯 : mobias
/// </summary>
#region 建築類型定義
public enum BuildingType
{
    None,       // 自由設定
    Hardwall,   // 不可破壞、不可互動
    Softwall,   // 可破壞、不可互動
    Mechanism   // 不可破壞、可互動
}
#endregion


public class Building : MonoBehaviour
{
    [Header("建築資訊")]
    public string buildingName;
    public BuildingType buildingType;

    [Header("屬性（自動依類型設定）")]
    public bool visionBlockage;
    public bool isDestructible;
    public bool isInteractable;

    #if UNITY_EDITOR
        private void OnValidate()
        {
            // 在編輯器中自動根據類型設定屬性
            if (buildingType == BuildingType.None) return;

            switch (buildingType)
            {
                case BuildingType.Hardwall:
                    visionBlockage = true;
                    isDestructible = false;
                    isInteractable = false;
                    break;
                case BuildingType.Softwall:
                    visionBlockage = true;
                    isDestructible = true;
                    isInteractable = false;
                    break;
                case BuildingType.Mechanism:
                    visionBlockage = false;
                    isDestructible = false;
                    isInteractable = true;
                    break;
            }
        }
    #endif
    //private void Start()
    //{
    //    Debug.Log("123")
    //}


    /// 執行互動邏輯（可從其他系統呼叫）
    public void Interact()
    {
        if (!isInteractable) return;

        Debug.Log($"{buildingName} 被互動了！");
        // TODO：在這裡觸發動畫、開門、開關機制等等
    }


    /// 接收攻擊（無血量，一擊摧毀）
    public void TakeDamage()
    {
        if (!isDestructible) return;

        Debug.Log($"{buildingName} 被攻擊，立即摧毀！");
        DestroyBuilding();

        
    }


    /// 執行破壞流程
    private void DestroyBuilding()
    {
        Debug.Log($"{buildingName} 被摧毀了！");
        // TODO：加入破壞動畫、粒子效果等
        Destroy(gameObject);
    }
}
