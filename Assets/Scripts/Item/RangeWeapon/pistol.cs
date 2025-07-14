using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// 小槍 - mobias
/// </summary>
public class Pistol : RangeWeapon
{

    public override void ItemInitialize(ItemData data)
    {
        base.ItemInitialize(data);
    }

    protected override void Start()
    {
        bulletCount = 3;
        base.Start(); // ✅ 正確呼叫父類 Start
    }

    protected override void Update()
    {
        base.Update(); // ✅ 正確呼叫父類 Update

    }



    public override void Attack()
    {
        Debug.Log("🔫 Pistol 的攻擊實作");
    }

    public override void AimTarget()
    {
        base.AimTarget();  // 呼叫父類邏輯，或你自己客製
    }
    public override void Use()
    {
        AimTarget(); // ✅ 呼叫自己的攻擊邏輯
    }
}
