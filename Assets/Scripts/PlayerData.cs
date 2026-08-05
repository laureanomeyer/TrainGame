using UnityEngine;

public class PlayerData
{
    private GameObject playerWeapon;
    public GameObject PlayerWeapon => playerWeapon;

    private float gold = 0;
    public float Gold => gold;

    public UnlockedLegado unlockedLegado;

    public PlayerData() 
    {
        unlockedLegado = new UnlockedLegado();
    }

    public void ChangeWeaponData(GameObject weapon)
    {
        playerWeapon = weapon;
    }

    public bool CheckWeapon()
    {
        if (playerWeapon == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void AddPlayerGold(float gold)
    {
        this.gold += gold;
    }

    public void ChangePlayerGold(float gold)
    {
        this.gold = gold;
    }

    public float GivePlayerGold()
    {
        return this.gold;
    }
    public void SpendGold(float amount)
    {
        gold = Mathf.Max(0, gold - amount);
    }

    public void ResetValuesToDefault()
    {
        playerWeapon = null;
        this.gold = 0;
        unlockedLegado.UnsuscribeEvents();
        unlockedLegado = new UnlockedLegado();
    }
}
