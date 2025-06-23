using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// 射程武器腳本 - mobias
/// </summary>
public class RangeWeapon : ItemScript
{
    public int bulletCount;

    protected List<Transform> selectedTargets = new List<Transform>();

    protected virtual void Start()
    {
        // ❗ 不建議在 Start() 呼叫 ItemInitialize()
    }

    protected virtual void Update()
    {
    }

    public override void Attack()
    {
        Debug.Log("🔫 預設攻擊邏輯（可被子類覆寫）");
    }

    protected virtual void changeToThrowWeapon()
    {
        // 預留轉換為投擲武器邏輯
    }

    /// <summary>
    /// 對外公開的瞄準流程（由 UIManager 呼叫）
    /// </summary>
    public virtual IEnumerator AimTarget()
    {
        Debug.Log("⌛ 進入選擇模式，請選擇目標，按【空白鍵】發射，或按【右鍵】取消");

        ClearSelection();

        while (true)
        {
            HandleSelectEnemy();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("✅ 確認發射！");
                Fire();
                break;
            }

            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("❌ 攻擊取消");
                ClearSelection();
                break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// 清空所有選擇的敵人
    /// </summary>
    public virtual void ClearSelection()
    {
        selectedTargets.Clear();
        Debug.Log("🧹 選取已清除！");
    }

    /// <summary>
    /// 發射攻擊
    /// </summary>
    public virtual void Fire()
    {
        Debug.Log($"🔫 發射！共攻擊 {selectedTargets.Count} 個目標");

        foreach (Transform enemy in selectedTargets)
        {
            Debug.Log($"➡ 攻擊敵人：{enemy.name}");
            // enemy.GetComponent<Enemy>()?.TakeDamage(damage);
        }

        bulletCount -= selectedTargets.Count;
        selectedTargets.Clear();
    }

    protected virtual void HandleSelectEnemy()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (selectedTargets.Count >= bulletCount)
            {
                Debug.Log("⚠ 子彈數量已滿，無法再選！");
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    selectedTargets.Add(hit.collider.transform);
                    Debug.Log("🎯 已選取敵人：" + hit.collider.name);
                }
            }
        }
    }

    public override void ItemInitialize(ItemData data)
    {
        base.ItemInitialize(data);
        bulletCount = data.damage;
        Debug.Log($"✅ RangeWeapon 初始化完成，彈藥數：{bulletCount}");
    }
}
