using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 射程武器腳本 - mobias
/// </summary>
public abstract class RangeWeapon : ItemScript
{
    public ItemData weaponSO;
    public int bulletCount;

    // ✅ 儲存所有選到的敵人（允許重複）
    protected List<Transform> selectedTargets = new List<Transform>();

    void Start()
    {
        ItemInitailize(weaponSO); // ✅ 修正拼字
    }

    void Update()
    {
        aimTarget(); // ✅ 呼叫瞄準與發射控制
    }

    void attack()
    {
        // 預留攻擊邏輯
    }

    void changToThrowWeapon()
    {
        // 預留轉換為投擲武器邏輯
    }

    /// <summary>
    /// 根據剩餘子彈進行選取與攻擊控制
    /// </summary>
    void aimTarget()
    {
        // ✅ 左鍵選擇敵人
        if (Input.GetMouseButtonDown(0))
        {
            if (selectedTargets.Count < bulletCount)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        selectedTargets.Add(hit.collider.transform);
                        Debug.Log("已選取敵人：" + hit.collider.name);
                    }
                }
            }
            else
            {
                Debug.Log("子彈數量已滿，無法再選！");
            }
        }

        // ✅ 右鍵取消所有選取
        if (Input.GetMouseButtonDown(1))
        {
            selectedTargets.Clear();
            Debug.Log("選取已清除！");
        }

        // ✅ 空白鍵發射
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("🔫 發射！總共發射 " + selectedTargets.Count + " 發");

            foreach (Transform enemy in selectedTargets)
            {
                Debug.Log("➡ 攻擊敵人：" + enemy.name);
                // 可執行敵人傷害函式，例如 enemy.GetComponent<Enemy>().TakeDamage()
            }

            bulletCount -= selectedTargets.Count;
            selectedTargets.Clear();
        }
    }

    /// <summary>
    /// 初始化武器設定（此方法可在子類覆寫）
    /// </summary>
    protected virtual void ItemInitialize(ItemData data)
    {
        Debug.Log("初始化武器：" + data.itemName);
    }
}
