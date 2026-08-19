using UnityEngine;

public enum WeaponType { Revolver, Rifle, Escopeta }

public interface IWeapons
{
    #region Variables
    public string Id { get; }

    public WeaponDataSO WeaponData { get; set; }
    BulletPool BulletPool { get; }

    public int CurrentAmmunition { get; set; }

    public bool IsReloading { get; set; }

    public float RateOfFire { get; }

    public float ReloadTime { get; }

    #endregion

    #region Funciones

    public void InitializeWeapon(BulletPool pool, PlayerAttackController playerAttack);
    public void DestroyWeapon();
    public void Tick(float deltaTime);
    public void Shoot(Transform spawnPoint);
    public void RestockBullets();
    public void Attack();
    public void ChargeTimers();
    public void ResetWaitToFire();
    public void RestockWeapon();

    #endregion

}
