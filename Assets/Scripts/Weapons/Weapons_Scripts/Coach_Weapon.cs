using UnityEngine;

public class Coach_Weapon : MonoBehaviour, IWeapons
{
    [Header("Name")]
    [SerializeField] private string weaponName;
    public string Id => weaponName;

    private PlayerAttackController playerAtkReference;

    [Header("Weapon data")]
    [SerializeField] private WeaponDataSO weaponData;

    public Transform weaponSpawnPoint;

    [Header("bullet dispersion")]
    [SerializeField] private int pelletCount;
    [SerializeField] private float spreadAngle;

    [Header("Bullet data")]
    [SerializeField] private BulletTypeScriptable bulletData;
    [SerializeField] private BulletTypeScriptable legadoBulletData;

    private BulletTypeScriptable currentBulletUse;

    [Header("Require Enemies Defetead")]
    [SerializeField] private int requireEnemyDefetead = 4;
    private int currentEnemiesDefetead = 0;

    [Header("Legacy charge data")]
    [Header("Mid level")]
    [SerializeField] private float midChargeTime = 0.5f;
    [SerializeField] private int midPelletCount = 4;
    [SerializeField] private float midSpreedRange = 40f;
    [SerializeField] private float midDamageMult = 1.5f;

    [Header("Max level")]
    [SerializeField] private float maxChargeTime = 1f;
    [SerializeField] private int maxPelletCount = 2;
    [SerializeField] private float maxSpreedRange = 20f;
    [SerializeField] private float maxDamageMult = 3f;

    private float currentCoachCharge = 0f;

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

    private bool unlockedLegacy = false;

    public void InitializeWeapon(BulletPool pool, PlayerAttackController playerAttack)
    {
        bulletPool = pool;
        playerAtkReference = playerAttack;

        var statsRef = ServiceLocator.Get<StatSystem>();
        rateOfFire = WeaponData.rateOfFire / statsRef.GetStat(StatType.AttackSpeed);
        reloadTime = WeaponData.reloadTime / statsRef.GetStat(StatType.AttackSpeed);

        EventBus.Subscribe<OnCoachDetectedDeadEnemy>(UpdateDefeteadEnemies);
        EventBus.Subscribe<OnUnlockCoachLegado>(UpdateCurrentBullet);

        PlayerData playerData = ServiceLocator.Get<PlayerData>();

        if (playerData.unlockedLegado.UnlockedCoach)
        {
            currentBulletUse = legadoBulletData;
            unlockedLegacy = true;
        }
        else
        {
            currentBulletUse = bulletData;
        }

        EventBus.Publish(new OnAmmoChangedEvent(currentAmmunition));
    }

    public void DestroyWeapon()
    {
        EventBus.Unsubscribe<OnCoachDetectedDeadEnemy>(UpdateDefeteadEnemies);
        EventBus.Unsubscribe<OnUnlockCoachLegado>(UpdateCurrentBullet);
        Debug.Log("Desuscribi evento");
    }

    public void Tick(float deltaTime)
    {
        ChargeTimers();

        if (unlockedLegacy == false)
        {
            if (playerAtkReference.IsAttacking)
            {
                Attack();
            }
        }
        else
        {
            if (waitToFire > rateOfFire)
            {
                if (IsReloading) return;

                if (playerAtkReference.IsAttacking == true)
                {
                    currentCoachCharge += Time.deltaTime;
                    Debug.Log(currentCoachCharge);
                }
                else if(playerAtkReference.IsAttacking == false && currentCoachCharge > 0)
                {
                    CalculateChargeAttack();
                    EventBus.Publish(new OnShootEvent(rateOfFire));
                    EventBus.Publish(new OnAmmoChangedEvent(currentAmmunition));
                    waitToFire = 0;
                }
            }
        }
    }

    private void CalculateChargeAttack()
    {
        if (currentCoachCharge < midChargeTime)
        {
            var data = WeaponData;
            currentBulletUse.Damage = data.damage;

            ReleaseChargeBullet(playerAtkReference.spawnPoint, pelletCount, spreadAngle);
            currentCoachCharge = 0;
            return;
        }
        else if (currentCoachCharge > midChargeTime && currentCoachCharge < maxChargeTime)
        {
            var data = WeaponData;
            currentBulletUse.Damage = data.damage * midDamageMult;

            ReleaseChargeBullet(playerAtkReference.spawnPoint, midPelletCount, midSpreedRange);
            currentCoachCharge = 0;
            return;
        }
        else if (currentCoachCharge > maxChargeTime)
        {
            var data = WeaponData;
            currentBulletUse.Damage = data.damage * maxDamageMult;

            ReleaseChargeBullet(playerAtkReference.spawnPoint, maxPelletCount, maxSpreedRange);
            currentCoachCharge = 0;
            return;
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

        if (unlockedLegacy == false)
        {
            if(currentEnemiesDefetead >= requireEnemyDefetead)
            {
                EventBus.Publish(new OnUpdatedCoachLegado());
                Debug.Log("Legado de Coach desbloqueado");
            }
            else
            {
                currentEnemiesDefetead = 0;
            }

            var data = WeaponData;
            currentBulletUse.Damage = data.damage;

            RealeasedBullet(weaponSpawnPoint);
        }
    }

    private void RealeasedBullet(Transform spawnPoint)
    {
        float angleStep = pelletCount > 1 ? spreadAngle / (pelletCount - 1) : 0f;
        float startAngle = -spreadAngle / 2f;

        for (int i = 0; i < pelletCount; i++)
        {
            float currentAngle = startAngle + angleStep * i;

            Quaternion spreadRotation = Quaternion.AngleAxis(currentAngle, Vector3.up);
            Quaternion finalRotation = spreadRotation * spawnPoint.rotation;

            BulletPool.ShootObject(spawnPoint.position, finalRotation, currentBulletUse);
        }

        CurrentAmmunition -= CurrentAmmunition;

        if (CurrentAmmunition == 0)
        {
            IsReloading = true;
            EventBus.Publish(new OnReloadEvent(reloadTime));
        }
    }

    private void ReleaseChargeBullet(Transform spawnPoint, int newPelletCount, float newSpreadAngle)
    {
        float angleStep = newPelletCount > 1 ? newSpreadAngle / (newPelletCount - 1) : 0f;
        float startAngle = -newSpreadAngle / 2f;

        for (int i = 0; i < newPelletCount; i++)
        {
            float currentAngle = startAngle + angleStep * i;

            Quaternion spreadRotation = Quaternion.AngleAxis(currentAngle, Vector3.up);
            Quaternion finalRotation = spreadRotation * spawnPoint.rotation;

            BulletPool.ShootObject(spawnPoint.position, finalRotation, currentBulletUse);
        }

        CurrentAmmunition -= CurrentAmmunition;

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

    private void UpdateDefeteadEnemies(OnCoachDetectedDeadEnemy updateEvent)
    {
        currentEnemiesDefetead += updateEvent.point;
    }

    private void UpdateCurrentBullet(OnUnlockCoachLegado unlockEvent)
    {
        currentBulletUse = legadoBulletData;
        unlockedLegacy = true;
}
}
