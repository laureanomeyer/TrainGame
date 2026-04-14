using UnityEngine;

public class Winchester : MonoBehaviour, IWeapons
{
    [Header("Name")]
    [SerializeField] private string weaponName;
    public string Id => weaponName;

    [Header("Type")]
    [SerializeField] private WeaponType weponType;
    public WeaponType WeaponType { get => weponType; }

    //Tipos de balas que utiliza el arma
    [Header("BulletType")]
    [SerializeField] private BulletTypeScriptable bulletScriptable;

    [Header("Ammun")]
    [SerializeField] private int bulletAmmunition;

    private int currentAmmunition;
    public float CurrentAmmunition { get => currentAmmunition; }

    [Header("Reloud time")]
    [SerializeField] private float reloadDuration;
    public float ReloadDuration { get => reloadDuration; }

    [Header("Rate of fire")]
    [SerializeField] private float rateOfFire;
    public float RateOfFire { get => rateOfFire; }


    [Header("Pool")]
    [SerializeField] private int maxCapacity;
    [SerializeField] private int defaultCapacity;


    //Referencia a la pool de balas
    private BulletPool bulletPool;

    public void Shoot(Transform spawnPoint)
    {
        if (currentAmmunition <= 0)
            return;

        bulletPool.ShootObject(spawnPoint.position, spawnPoint.rotation, bulletScriptable);
        currentAmmunition -= 1;

    }

    public void SetPool(BulletPool pool)
    {
        bulletPool = pool;

        pool.MaxCapacity = maxCapacity;
        pool.DefaultCapacity = defaultCapacity;
    }

    public void Reload()
    {
        currentAmmunition = bulletAmmunition;
    }
}
