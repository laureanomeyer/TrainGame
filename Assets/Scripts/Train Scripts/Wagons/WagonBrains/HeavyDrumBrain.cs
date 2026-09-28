using UnityEngine;

public class HeavyDrumBrain : WagonBrain
{
    private IWeapons playerWeapon;

    [Header("Damage Buffer")]
    [SerializeField] private float damageUpgrade;

    private void Awake()
    {
        WagonType = WagonType.Passive;
    }

    private void OnTriggerEnter(Collider other)
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
                    weaponBuffer.BufferRoF(damageUpgrade);
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
