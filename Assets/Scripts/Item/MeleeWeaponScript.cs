using UnityEngine;
/// <summary>
/// 近戰武器腳本 - Jerry0401
/// </summary>
public class MeleeWeapon : ItemScript
{
    public ItemData weaponSO;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ItemInitailize(weaponSO);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Attack()
    {
        throw new System.NotImplementedException();
    }
    
}
