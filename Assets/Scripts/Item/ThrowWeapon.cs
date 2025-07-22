using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/// <summary>
/// 投擲道具 - mobias
/// </summary>
public class ThrowWeapon : ItemScript
{
    public int throwRange = 3;             // 可投擲格數
    // public int damage = 1;                 // 傷害值
    public GameObject throwEffect;         // 擊中特效（選用）
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

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Enemy"))
        {
            selectedTargets.Add(hit.collider.transform);
            Debug.Log("🎯 已選取敵人：" + hit.collider.name);
            Fire();
        }
    }

    public virtual void Fire()
    {

        foreach (Transform enemy in selectedTargets)
        {
            Debug.Log($"➡ 攻擊敵人：{enemy.name}");
            // enemy.GetComponent<Enemy>()?.TakeDamage(damage);
        }
        selectedTargets.Clear();
        selectEnemy = false;
        playerScript.isCardDragging = false;
        RemoveItemFromPocket();

    }   
    public virtual void AimTarget()
    {
        selectedTargets.Clear();
        selectEnemy = true;

    }
    public override void Attack()
    {

    }

    public override void ItemInitialize(ItemData data)
    {
        base.ItemInitialize(data);
    }

    public override void CancelAttackAndRestore()
    {
        base.CancelAttackAndRestore();
        selectedTargets.Clear();
        selectEnemy = false;

    }

}
