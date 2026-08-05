using System;
using UnityEngine;

public class Winchester_Weapon : MonoBehaviour, IWeapons
{
    [Header("Name")]
    [SerializeField] private string weaponName;
    public string Id => weaponName;

    private PlayerAttackController playerAtkReference;

    [Header("Weapon data")]
    [SerializeField] private WeaponDataSO weaponData;

    [Header("Bullet data")]
    [SerializeField] private BulletTypeScriptable bulletData;
    public WeaponDataSO WeaponData { get => weaponData; set => weaponData = value; }

    private int currentAmmunition;
    public int CurrentAmmunition { get => currentAmmunition; set => currentAmmunition = value; }

    private bool isReloading = false;
    public bool IsReloading { get => isReloading; set => isReloading = value; }

    private float waitToFire = 0;

    private float rateOfFire;
    public float RateOfFire { get => rateOfFire; }

    private float currentReloadTime = 0;

    private float reloadTime;
    public float ReloadTime { get => reloadTime; }

    //Referencia a la pool de balas
    private BulletPool bulletPool;
    public BulletPool BulletPool => bulletPool;

    public void InitializeWeapon(BulletPool pool, PlayerAttackController playerAttack)
    {
        bulletPool = pool;
        playerAtkReference = playerAttack;

        EventBus.Subscribe<OnDetectedDeadEnemy>(CallRestock);

        var statsRef = ServiceLocator.Get<StatSystem>();
        rateOfFire = WeaponData.rateOfFire / statsRef.GetStat(StatType.AttackSpeed);
        reloadTime = WeaponData.reloadTime / statsRef.GetStat(StatType.AttackSpeed);
    }

    public void Tick(float deltaTime)
    {
        ChargeTimers();

        if (playerAtkReference.IsAttacking)
        {
            Attack();
        }
    }

    public void Shoot(Transform spawnPoint)
    {
        if (IsReloading) return;
        if (spawnPoint == null) return;

        var data = WeaponData;
        bulletData.Damage = data.damage;
        BulletPool.ShootObject(spawnPoint.position, spawnPoint.rotation, bulletData);

        CurrentAmmunition -= 1;

        if (CurrentAmmunition == 0)
        {
            IsReloading = true;
            EventBus.Publish(new OnReloadEvent(reloadTime));
        }
    }

    public void RestockBullets()
    {
        currentAmmunition = weaponData.ammun;
    }

    public void Attack()
    {
        if (waitToFire > rateOfFire)
        {
            if (IsReloading) return;

            Shoot(playerAtkReference.spawnPoint);
            EventBus.Publish(new OnShootEvent(rateOfFire));
            EventBus.Publish(new OnAmmoChangedEvent(currentAmmunition));
            waitToFire = 0;
        }
    }
    public void ChargeTimers()
    {
        if (waitToFire <= rateOfFire)
        {
            waitToFire += Time.deltaTime;
        }

        if (IsReloading)
        {
            currentReloadTime += Time.deltaTime;

            if (currentReloadTime > reloadTime)
            {
                RestockWeapon();
            }
        }

    }
    public void ResetWaitToFire()
    {
        EventBus.Publish(new OnShootEvent(rateOfFire));
        EventBus.Publish(new OnAmmoChangedEvent(currentAmmunition));
        waitToFire = 0;
    }

    public void RestockWeapon()
    {
        currentReloadTime = 0;
        RestockBullets();
        IsReloading = false;
        AudioManager.Instance.Play($"RevolverMusketReload{1}");
        EventBus.Publish(new OnAmmoChangedEvent(currentAmmunition));
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnDetectedDeadEnemy>(CallRestock);
        Debug.Log("Desuscribi evento");
    }

    private void CallRestock(OnDetectedDeadEnemy enemyEvent)
    {
        EventBus.Publish(new OnReloadEvent(0.2f));
        RestockWeapon();
        ResetWaitToFire();
    }
}
