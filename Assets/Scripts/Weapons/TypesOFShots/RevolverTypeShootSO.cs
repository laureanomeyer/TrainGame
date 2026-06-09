using UnityEngine;

[CreateAssetMenu(fileName = "New Shoot", menuName = "Weapons/Type of shoot/Revolver type of shoot")]
public class RevolverTypeShootSO : TypeOfShoot
{
    public override void Shoot(IWeapons weapon, Transform spawnPoint, PlayerAttackController playerRefence)
    {
        if (weapon.IsReloading) return;
        if (spawnPoint == null) return;

        var data = weapon.WeaponData;
        data.bulletSO.Damage = data.damage;
        weapon.BulletPool.ShootObject(spawnPoint.position, spawnPoint.rotation, data.bulletSO);

        weapon.CurrentAmmunition -= 1;


        if (weapon.CurrentAmmunition == 0)
        {
            weapon.IsReloading = true;
            GameEvents.ReloadStarted(playerRefence.ReloadTime);
        }
    }
}
