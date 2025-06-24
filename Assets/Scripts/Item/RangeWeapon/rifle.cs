using UnityEngine;
using System.Collections;
/// <summary>
/// 步槍 - mobias
/// </summary>
public class Rifle : RangeWeapon
{

    public override void ItemInitialize(ItemData data)
    {
        base.ItemInitialize(data);
        bulletCount = 2; // 強制使用 Pistol 固定子彈數
    }

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
        Debug.Log("🔫 Rifle 的攻擊實作");
    }

    public override IEnumerator AimTarget()
    {
        Debug.Log("🔫 Pistol 的選取流程開始");

        yield return base.AimTarget();  // 呼叫父類邏輯，或你自己客製
    }
    public override void Use()
    {
        Debug.Log("🟢 Rifle.Use() 被呼叫！");
        StartCoroutine(AimTarget()); // ✅ 呼叫自己的攻擊邏輯
    }
}

