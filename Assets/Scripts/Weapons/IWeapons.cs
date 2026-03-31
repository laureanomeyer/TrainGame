using UnityEngine;

public interface IWeapons
{
    //Nombre de identificacion de armas
    public string id { get; }

    public float RateOfFire { get; }

    public float ReloadDuration { get; }

    public float CurrentAmmunition { get; }

    //Funcion de ataque, requiere un punto de spawneo de balas
    public void Shoot(Transform spawnPoint);

    //Establece la pool
    public void SetPool(BulletPool pool);

    public void Reload();

}
