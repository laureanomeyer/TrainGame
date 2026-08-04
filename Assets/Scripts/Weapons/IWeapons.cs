using UnityEngine;

public enum WeaponType { Revolver, Rifle, Escopeta }

public interface IWeapons
{
    //Nombre de identificacion de armas
    public string Id { get; }

    public WeaponDataSO WeaponData { get; set; }
    BulletPool BulletPool { get; }

    public int CurrentAmmunition { get; set; }

    public bool IsReloading { get; set; }

    //Funcion de ataque, requiere un punto de spawneo de balas
    public void Shoot(Transform spawnPoint);

    public void InitializeWeapon(BulletPool pool, PlayerAttackController playerAttack);

    public void RestockBullets();

}
