using UnityEngine;

public class SpecerRifle_Weapon : MonoBehaviour, IWeapons
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

    [Header("Enemies to defeat for legado")]
    [SerializeField] private int EnemiesToDefeat = 20;

    private int currentEnemiesDefetead;

    [Header("Time to defeat enemies")]
    [SerializeField] private float defeatEnemiesTime = 5f;

    private float currentUnlockTime;

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

    //Referencia a la pool de balas
    private BulletPool bulletPool;
    public BulletPool BulletPool => bulletPool;

    private bool unlockedLegado = false;

    public void InitializeWeapon(BulletPool pool, PlayerAttackController playerAttack)
    {
        bulletPool = pool;
        playerAtkReference = playerAttack;

        EventBus.Subscribe<OnSpencerDetectedDeadEnemy>(CheckEnemiesDefetead);
        EventBus.Subscribe<OnUnlockSpencerLegado>(UpdateCurrentBullet);

        var statsRef = ServiceLocator.Get<StatSystem>();
        rateOfFire = WeaponData.rateOfFire / statsRef.GetStat(StatType.AttackSpeed);
        reloadTime = WeaponData.reloadTime / statsRef.GetStat(StatType.AttackSpeed);

        PlayerData playerData = ServiceLocator.Get<PlayerData>();

        currentUnlockTime = 0;

        if (playerData.unlockedLegado.UnlockedSpencer)
        {
            currentBulletUse = legadoBulletData;
            unlockedLegado = true;
        }
        else
        {
            currentBulletUse = bulletData;
        }
    }

    public void DestroyWeapon()
    {
        EventBus.Unsubscribe<OnSpencerDetectedDeadEnemy>(CheckEnemiesDefetead);
        EventBus.Unsubscribe<OnUnlockSpencerLegado>(UpdateCurrentBullet);
        Debug.Log("Desuscribi evento");
    }

    public void Tick(float deltaTime)
    {
        if (!unlockedLegado)
        {
            PlayerData playerData = ServiceLocator.Get<PlayerData>();
            if (playerData.unlockedLegado.UnlockedSpencer == false)
            {
                CalculetUnlockLegado();
            }
        }
        
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
        AudioManager.Instance.Play($"SFXMusketReloaded");
        EventBus.Publish(new OnAmmoChangedEvent(currentAmmunition));
    }

    private void CheckEnemiesDefetead(OnSpencerDetectedDeadEnemy checkEnemies)
    {
        currentEnemiesDefetead += 1;
    }

    private void CalculetUnlockLegado()
    {
        if(currentEnemiesDefetead > 0)
        {
            currentUnlockTime -= Time.deltaTime;

            if (currentUnlockTime > defeatEnemiesTime)
            {
                currentUnlockTime = 0f;
                currentEnemiesDefetead = 0;
            }

            if (currentEnemiesDefetead >= EnemiesToDefeat)
            {
                EventBus.Publish(new OnUpdatedSpencerLegado());
            }
        }
    }

    private void UpdateCurrentBullet(OnUnlockSpencerLegado unlockEvent)
    {
        currentBulletUse = legadoBulletData;
    }
}
