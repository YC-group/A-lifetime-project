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

    private InputSystemActions inputActions; // ✅ 改用類別方式

    protected virtual void Start()
    {
        inputActions = new InputSystemActions(); // ✅ 建立實例
        // 綁定事件
        inputActions.Item.Fire.performed += OnFire;
        inputActions.Item.Cancel.performed += OnCancel;
        inputActions.Item.SelectTarget.performed += OnSelectTarget;
        inputActions.Enable(); // ✅ 啟用整組 input
    }

    protected virtual void OnDisable()
    {

        inputActions.Item.Fire.performed -= OnFire;
        inputActions.Item.Cancel.performed -= OnCancel;
        inputActions.Item.SelectTarget.performed -= OnSelectTarget;
        inputActions.Disable();
    }

    protected virtual void Update()
    {
        if (selectEnemy)
        {
            HandleSelectEnemy();
        }
    }

    private void OnFire(InputAction.CallbackContext ctx)
    {
        if (!selectEnemy) return;

        Debug.Log("✅ 確認發射！");
        Fire();
        Debug.Log("剩餘子彈：" + bulletCount);
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (!selectEnemy) return;

        Debug.Log("❌ 攻擊取消");
        CancelAttackAndRestore();
    }

    private void OnSelectTarget(InputAction.CallbackContext ctx)
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

    protected virtual void changeToThrowWeapon() { }

    public virtual void AimTarget()
    {
        selectedTargets.Clear();
        selectEnemy = true;
        if (cardCanvasGroup == null)
            Debug.LogWarning("⚠ cardCanvasGroup 尚未正確初始化！");
    }

    public void RestoreCardDisplay()
    {
        if (cardCanvasGroup == null) return;
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
        dragHandler?.ResetUsedFlag();
        UIManager.Instance?.UnlockCardAndPlayer();
    }

    public virtual void Fire()
    {
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
        UIManager.Instance?.UnlockCardAndPlayer();
    }

    protected virtual void HandleSelectEnemy() { }

    public override void ItemInitialize(ItemData data)
    {
        base.ItemInitialize(data);
    }
}
