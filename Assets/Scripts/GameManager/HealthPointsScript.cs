using UnityEngine;

/// <summary>
/// 血量計算腳本 - Jerry0401
/// </summary>
public class HealthPointsScript : MonoBehaviour
{
    private static HealthPointsScript Instance;
    
    public static HealthPointsScript GetInstance()  // Singleton
    {
        if (Instance == null)
        {
            Instance = GameObject.FindAnyObjectByType<HealthPointsScript>();
            if (Instance == null)
            {
                Debug.LogError("No HealthPointsScript found");
                return null;
            }
        }
        return Instance;
    }
    
    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("已存在其他 HealthPointsScript 實例，將刪除此物件。");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    private void OnDestroy()
    {
        // Singleton
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 受到近戰攻擊
    /// </summary>
    /// <returns>float</returns>
    public float TakeMeleeDamage(float hp)
    {
        hp--;
        return hp;
    }
}
