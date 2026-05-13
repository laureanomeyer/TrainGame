
using UnityEngine;
using UnityEngine.Rendering;

public class Revolver : MonoBehaviour, IWeapons
{
    [Header("Name")]
    [SerializeField] private string weaponName;
    public string Id => weaponName;

    [Header("Type")]
    [SerializeField] private WeaponType weponType;
    public WeaponType WeaponType { get => weponType; }

    [Header("Damage")]
    [SerializeField] private float damage;
    public float Damage { get => damage; set => damage = value; }

    //Tipos de balas que utiliza el arma
    [Header("BulletType")]
    [SerializeField] private BulletTypeScriptable bulletScriptable;

    [Header("Ammun")]
    [SerializeField] private int bulletAmmunition;

    private int currentAmmunition;
    public float CurrentAmmunition { get => currentAmmunition; }

    [Header("Reloud time")]
    [SerializeField] private float reloadDuration;
    public float ReloadDuration { get => reloadDuration; set => reloadDuration = value; }

    [Header("Rate of fire")]
    [SerializeField] private float rateOfFire;
    public float RateOfFire { get => rateOfFire; set => rateOfFire = value; }

    private bool isReloading =false;
    public bool IsReloading { get => isReloading; set => isReloading = value; }


    //Referencia a la pool de balas
    private BulletPool bulletPool;

    public void Shoot(Transform spawnPoint)
    {
        if (isReloading) return;
        if (spawnPoint != null) 
        {
            bulletScriptable.Damage = damage;
            bulletPool.ShootObject(spawnPoint.position, spawnPoint.rotation, bulletScriptable);
            currentAmmunition -= 1;
        }
       

        if (currentAmmunition <= 0)
        {
            isReloading = true;
        }
    }

    public void SetPool(BulletPool pool)
    {
        bulletPool = pool;
    }



    public void RestockBullets()
    {
        currentAmmunition = bulletAmmunition;
    }
}
