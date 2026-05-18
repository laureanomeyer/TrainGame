using UnityEngine;

public abstract class TypeOfShoot : ScriptableObject
{
    public abstract void Shoot(IWeapons weapon, Transform spawnPoint, BulletPool pool, WeaponDataSO weaponData);
}
