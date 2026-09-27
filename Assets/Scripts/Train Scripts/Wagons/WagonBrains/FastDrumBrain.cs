using UnityEngine;

public class FastDrumBrain : WagonBrain
{
    private IWeapons playerWeapon;

    [Header("Rate of Fire Buffer")]
    [SerializeField] private float rofUpgrade;

    private void Awake()
    {
        WagonType = WagonType.Passive;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if(playerWeapon == null)
            {
                PlayerBrain playerRef = other.GetComponent<PlayerBrain>();
                playerWeapon = playerRef.PlayerAttackController.Weapon;
            }

            if(playerWeapon.WeaponData.type == WeaponType.Revolver)
            {
                if (playerWeapon is IWeaponBuffer weaponBuffer)
                {
                    weaponBuffer.BufferRoF(rofUpgrade);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (playerWeapon == null)
            {
                PlayerBrain playerRef = other.GetComponent<PlayerBrain>();
                playerWeapon = playerRef.PlayerAttackController.Weapon;
            }

            if (playerWeapon.WeaponData.type == WeaponType.Revolver)
            {
                if (playerWeapon is IWeaponBuffer weaponBuffer)
                {
                    weaponBuffer.DebuffRoF();
                }
            }
        }
    }
}
