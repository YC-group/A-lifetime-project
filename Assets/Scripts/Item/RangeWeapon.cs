using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// 射程武器腳本 - mobias
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

        if (selectEnemy)
        {
            HandleSelectEnemy();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("✅ 確認發射！");
                Fire();
            }

            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("❌ 攻擊取消");
                CancelAttackAndRestore();
            }

        }
    }

    public override void Attack()
    {
        Debug.Log("🔫 預設攻擊邏輯（可被子類覆寫）");
    }

    protected virtual void changeToThrowWeapon()
    {
        // 預留轉換為投擲武器邏輯
    }


    public virtual void AimTarget()
    {
        selectedTargets.Clear();
        selectEnemy = true;
        if (cardCanvasGroup == null)
            Debug.LogWarning("⚠ cardCanvasGroup 尚未正確初始化！");
    }

    public void RestoreCardDisplay()
    {
        
        Debug.Log("🎴 成功恢復卡片！");
        cardCanvasGroup.alpha = 1f;
        cardCanvasGroup.interactable = true;
        cardCanvasGroup.blocksRaycasts = true;

    }

    public void CancelAttackAndRestore()
    {
        selectedTargets.Clear();
        selectEnemy = false;
        RestoreCardDisplay();
        var dragHandler = GetComponent<CardDragHandler>();
        if (dragHandler != null)
        {
            dragHandler.ResetUsedFlag();
        }
    }


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
        selectEnemy = false;
        if (bulletCount > 0)
        {
            CancelAttackAndRestore();
        }
        else
        {
            Destroy(gameObject);
        }
            

    }

    protected virtual void HandleSelectEnemy()
    {

        if (Input.GetMouseButtonDown(0))
        {
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
    }
}
