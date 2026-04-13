using UnityEngine;

public class PlayerData
{
    private GameObject playerWeapon;
    public GameObject PlayerWeapon => playerWeapon;

    private float playerGold;

    public PlayerData() { }

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
        playerGold += gold;
    }

    public void ChangePlayerGold(float gold)
    {
        playerGold = gold;
    }

    public float GivePlayerGold()
    {
        return playerGold;
    }

    public void ResetValuesToDefault()
    {
        playerWeapon = null;
        playerGold = 0;
    }
}
