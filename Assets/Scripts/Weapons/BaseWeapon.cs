
using UnityEngine;
using UnityEngine.Rendering;

public class BaseWeapon : MonoBehaviour, IWeapons
{
    [Header("Name")]
    [SerializeField] private string weaponName;
    public string Id => weaponName;

    private PlayerAttackController playerAtkReference;

    [Header("Weapon data")]
    [SerializeField] private WeaponDataSO weaponData;
    public WeaponDataSO WeaponData { get => weaponData; set => weaponData = value; }

    private int currentAmmunition;
    public int CurrentAmmunition { get => currentAmmunition; set => currentAmmunition = value; }

    private bool isReloading =false;
    public bool IsReloading { get => isReloading; set => isReloading = value; }

    //Referencia a la pool de balas
    private BulletPool bulletPool;
    public BulletPool BulletPool => bulletPool;

    public void Shoot(Transform spawnPoint)
    {
        //weaponData.typeOfShootSO.Shoot(this, spawnPoint, playerAtkReference);
        if (IsReloading) return;
        if (spawnPoint == null) return;

        var data = WeaponData;
        data.bulletSO.Damage = data.damage;
        BulletPool.ShootObject(spawnPoint.position, spawnPoint.rotation, data.bulletSO);

        CurrentAmmunition -= 1;

        if (CurrentAmmunition == 0)
        {
            IsReloading = true;
            EventBus.Publish(new OnReloadEvent(playerAtkReference.ReloadTime));
        }
    }

    public void InitializeWeapon(BulletPool pool, PlayerAttackController playerAttack)
    {
        bulletPool = pool;
        playerAtkReference = playerAttack;
    }

    public void RestockBullets()
    {
        currentAmmunition = weaponData.ammun;
    }
}
