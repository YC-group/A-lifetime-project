using UnityEngine;
/// <summary>
/// 近戰武器腳本 - Jerry0401
/// </summary>
public abstract class MeleeWeapon : ItemScript
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
}
