using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 小槍 - mobias
/// </summary>
public class Pistol : RangeWeapon
{

    protected override void Start()
    {
        bulletCount = 2;
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
}
