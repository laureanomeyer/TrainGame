using System;
using System.Collections.Generic;
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

    [Header("Legado Bullet data")]
    [SerializeField] private BulletTypeScriptable legadoBulletData;

    private BulletTypeScriptable currentBulletUse;

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

    private List<Enemy> enemiesDamage = new List<Enemy>();

    private BulletPool bulletPool;
    public BulletPool BulletPool => bulletPool;

    public void InitializeWeapon(BulletPool pool, PlayerAttackController playerAttack)
    {
        bulletPool = pool;
        playerAtkReference = playerAttack;

        EventBus.Subscribe<OnUpdateEnemiesDamage>(UpdateEnemiesDamage);
        EventBus.Subscribe<OnUnlockWinchesterLegado>(UpdateCurrentBullet);
        EventBus.Subscribe<OnWinchesterDetectedDeadEnemy>(CallRestock);

        var statsRef = ServiceLocator.Get<StatSystem>();
        rateOfFire = WeaponData.rateOfFire / statsRef.GetStat(StatType.AttackSpeed);
        reloadTime = WeaponData.reloadTime / statsRef.GetStat(StatType.AttackSpeed);

        PlayerData playerData = ServiceLocator.Get<PlayerData>();

        if (playerData.unlockedLegado.UnlockedWinchester)
        {
            currentBulletUse = legadoBulletData;
        }
        else
        {
            currentBulletUse = bulletData;
        }
    }

    public void DestroyWeapon()
    {
        EventBus.Unsubscribe<OnUpdateEnemiesDamage>(UpdateEnemiesDamage);
        EventBus.Unsubscribe<OnUnlockWinchesterLegado>(UpdateCurrentBullet);
        EventBus.Unsubscribe<OnWinchesterDetectedDeadEnemy>(CallRestock);
        Debug.Log("Desuscribi evento");
    }

    public void Tick(float deltaTime)
    {
        ChargeTimers();

        if (playerAtkReference.IsAttacking)
        {
            Attack();
        }
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

    public void Shoot(Transform spawnPoint)
    {
        if (IsReloading) return;
        if (spawnPoint == null) return;

        if(currentBulletUse.typeOfCollsion is CheckWinchesterCollsion winchesterCollsion)
        {
            winchesterCollsion = currentBulletUse.typeOfCollsion as CheckWinchesterCollsion;
            winchesterCollsion.enemiesDamage = enemiesDamage;
            currentBulletUse.typeOfCollsion = winchesterCollsion;
        }

        var data = WeaponData;
        currentBulletUse.Damage = data.damage;
        BulletPool.ShootObject(spawnPoint.position, spawnPoint.rotation, currentBulletUse);

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
        AudioManager.Instance.Play($"SFXMusketReloaded");
        EventBus.Publish(new OnAmmoChangedEvent(currentAmmunition));
    }

    private void UpdateEnemiesDamage(OnUpdateEnemiesDamage eventEnemiesDamage)
    {
        enemiesDamage = eventEnemiesDamage.Enemies;
    }

    private void CallRestock(OnWinchesterDetectedDeadEnemy enemyEvent)
    {
        EventBus.Publish(new OnReloadEvent(0.2f));
        RestockWeapon();
        ResetWaitToFire();
    }

    private void UpdateCurrentBullet(OnUnlockWinchesterLegado unlockEvent)
    {
        currentBulletUse = legadoBulletData;
    }
}
