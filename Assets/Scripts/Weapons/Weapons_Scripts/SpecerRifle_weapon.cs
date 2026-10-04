using UnityEngine;

public class SpecerRifle_Weapon : MonoBehaviour, IWeapons
{
    [Header("Name")]
    [SerializeField] private string weaponName;
    public string Id => weaponName;

    private PlayerAttackController playerAtkReference;

    [Header("Weapon data")]
    [SerializeField] private WeaponDataSO weaponData;

    public Transform weaponSpawnPoint;

    [Header("Particle Sequence Controller")]
    [SerializeField] ParticleSequenceController sequenceController;

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

    private float damage;

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
        EventBus.Subscribe<OnStatChangedEvent>(UpdateStats);

        var statsRef = ServiceLocator.Get<StatSystem>();
        rateOfFire = WeaponData.rateOfFire / statsRef.GetStat(StatType.AttackSpeed);
        reloadTime = WeaponData.reloadTime / statsRef.GetStat(StatType.AttackSpeed);
        damage = WeaponData.damage * statsRef.GetStat(StatType.DamageMultiplier);

        PlayerData playerData = ServiceLocator.Get<PlayerData>();

        currentUnlockTime = 0;

        if (playerData.unlockedLegado.UnlockedSpencer)
        {
            currentBulletUse = legadoBulletData;
            unlockedLegado = true;
            sequenceController.PlayGroup(ParticleGroups.LegacyUnlocked);
        }
        else
        {
            currentBulletUse = bulletData;
        }

        EventBus.Publish(new OnAmmoChangedEvent(currentAmmunition));
    }

    public void DestroyWeapon()
    {
        EventBus.Unsubscribe<OnSpencerDetectedDeadEnemy>(CheckEnemiesDefetead);
        EventBus.Unsubscribe<OnUnlockSpencerLegado>(UpdateCurrentBullet);
        EventBus.Unsubscribe<OnStatChangedEvent>(UpdateStats);
        Debug.Log("Desuscribi evento " + gameObject.name);
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

    public void Shoot()
    {
        if (IsReloading) return;
        if (weaponSpawnPoint == null) return;

        currentBulletUse.Damage = damage;
        BulletPool.ShootObject(weaponSpawnPoint.position, weaponSpawnPoint.rotation, currentBulletUse);

        sequenceController.Play("shotParticles");

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

            Shoot();
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
        unlockedLegado = true;
        sequenceController.PlayGroup(ParticleGroups.LegacyUnlocked);
    }

    public void UpdateStats(OnStatChangedEvent @event)
    {
        var statsRef = ServiceLocator.Get<StatSystem>();

        //Rate of fire set up
        rateOfFire = WeaponData.rateOfFire / statsRef.GetStat(StatType.AttackSpeed);

        //Reload time set up
        reloadTime = WeaponData.reloadTime / statsRef.GetStat(StatType.AttackSpeed);

        //Damage set up
        damage = WeaponData.damage * statsRef.GetStat(StatType.DamageMultiplier);

        Debug.Log("Stats actualizadas");
    }
}
