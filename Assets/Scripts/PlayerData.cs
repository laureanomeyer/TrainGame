using UnityEngine;

public class PlayerData
{
    private GameObject playerWeapon;
    public GameObject PlayerWeapon => playerWeapon;

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
}
