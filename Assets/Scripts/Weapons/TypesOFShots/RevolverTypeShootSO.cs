using UnityEngine;

[CreateAssetMenu(fileName = "New Shoot", menuName = "Weapons/Type of shoot/Revolver type of shoot")]
public class RevolverTypeShootSO : TypeOfShoot
{
    public override void Shoot(IWeapons weapon, Transform spawnPoint, BulletPool pool, WeaponDataSO weaponData)
    {
        if (weapon.IsReloading) return;
        if (spawnPoint != null)
        {
            weaponData.bulletSO.Damage = weaponData.damage;
            pool.ShootObject(spawnPoint.position, spawnPoint.rotation, weaponData.bulletSO);

            weapon.CurrentAmmunition -= 1;
        }

        if (weapon.CurrentAmmunition <= 0)
        {
            weapon.IsReloading = true;
        }
    }
}
