
using UnityEngine;
using UnityEngine.Rendering;

public class BaseWeapon : MonoBehaviour, IWeapons
{
    [Header("Name")]
    [SerializeField] private string weaponName;
    public string Id => weaponName;

    [Header("Weapon data")]
    [SerializeField] private WeaponDataSO weaponData;
    public WeaponDataSO WeaponData { get => weaponData; set => weaponData = value; }

    private int currentAmmunition;
    public int CurrentAmmunition { get => currentAmmunition; set => currentAmmunition = value; }

    private bool isReloading =false;
    public bool IsReloading { get => isReloading; set => isReloading = value; }


    //Referencia a la pool de balas
    private BulletPool bulletPool;

    public void Shoot(Transform spawnPoint)
    {
        weaponData.typeOfShootSO.Shoot(this, spawnPoint, bulletPool, weaponData);
    }

    public void SetPool(BulletPool pool)
    {
        bulletPool = pool;
    }

    public void RestockBullets()
    {
        currentAmmunition = weaponData.ammun;
    }
}
