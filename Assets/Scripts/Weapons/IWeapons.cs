using UnityEngine;

public enum WeaponType { Revolver, Rifle, Escopeta }

public interface IWeapons
{
    //Nombre de identificacion de armas
    public string Id { get; }

    public WeaponType WeaponType { get; }

    public float RateOfFire { get; set; }

    public float ReloadDuration { get; set; }

    public float CurrentAmmunition { get; }

    public bool IsReloading { get; set; }

    //Funcion de ataque, requiere un punto de spawneo de balas
    public void Shoot(Transform spawnPoint);

    //Establece la pool
    public void SetPool(BulletPool pool);

    public void RestockBullets();

}
