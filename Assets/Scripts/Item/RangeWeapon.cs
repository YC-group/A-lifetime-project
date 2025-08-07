using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 射程武器腳本 - mobias (使用自動產生的 InputSystemActions 類別)
/// </summary>
public class RangeWeapon : ItemScript
{

    public int bulletCount;
    public bool selectEnemy = false;
    protected List<Transform> selectedTargets = new List<Transform>();

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    public void SelectTarget()
    {
        if (!selectEnemy) return;

        if (bulletCount <= 0)
        {
            Debug.Log("⚠️ 沒有子彈了，不能選擇敵人");
            return;
        }

        if (selectedTargets.Count >= bulletCount)
        {
            Debug.Log("⚠️ 選擇數已達最大（依照子彈數）");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Enemy"))
        {
            selectedTargets.Add(hit.collider.transform);
            Debug.Log("🎯 已選取敵人：" + hit.collider.name);
        }
    }

    public override void Attack()
    {
        Debug.Log("🔫 預設攻擊邏輯（可被子類覆寫）");
    }


    public virtual void AimTarget()
    {
        selectedTargets.Clear();
        selectEnemy = true;

    }
    public void CancelAttackAndRestore()
    {
        base.CancelAttackAndRestore();
        selectedTargets.Clear();
        selectEnemy = false;
    }

    public virtual void Fire()
    {
        Debug.Log("✅ 確認發射！");
        
        foreach (Transform enemy in selectedTargets)
        {
            Debug.Log($"➡ 攻擊敵人：{enemy.name}");
            // enemy.GetComponent<Enemy>()?.TakeDamage(damage);
        }

        bulletCount -= selectedTargets.Count;
        selectedTargets.Clear();
        selectEnemy = false;
        playerScript.isCardDragging = false;
        if (bulletCount > 0)
        {
            CancelAttackAndRestore();
        }
        else
        {
            RemoveItemFromPocket();
        }
        Debug.Log("剩餘子彈：" + bulletCount);
    }



    public override void ItemInitialize(ItemData data)
    {
        base.ItemInitialize(data);

    }
}
