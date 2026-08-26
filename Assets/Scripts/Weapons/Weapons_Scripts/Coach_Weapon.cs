using UnityEngine;

public class Coach_Weapon : MonoBehaviour, IWeapons
{
    [Header("Name")]
    [SerializeField] private string weaponName;
    public string Id => weaponName;

    private PlayerAttackController playerAtkReference;

    [Header("Weapon data")]
    [SerializeField] private WeaponDataSO weaponData;

    [Header("Pellet count")]
    [SerializeField] private int pelletCount;

    [Header("Spreed Angle")]
    [SerializeField] private float spreadAngle;

    [Header("Bullet data")]
    [SerializeField] private BulletTypeScriptable bulletData;

    [Header("Legado Bullet data")]
    [SerializeField] private BulletTypeScriptable legadoBulletData;

    private BulletTypeScriptable currentBulletUse;

    [Header("Require Enemies Defetead")]
    [SerializeField] private int requireEnemyDefetead = 4;

    private int currentEnemiesDefetead = 0;

    [Header("Max Coach Charge")]
    [SerializeField] private float maxCoachCharge = 1.5f;

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


    private bool unlockedLegado = false;

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
            unlockedLegado = true;
        }
        else
        {
            currentBulletUse = bulletData;
        }
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

        if (playerAtkReference.IsAttacking)
        {
            Attack();
        }
    }

    public void Shoot(Transform spawnPoint)
    {
        if (IsReloading) return;
        if (spawnPoint == null) return;

        if (unlockedLegado == false)
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
        }

        var data = WeaponData;
        currentBulletUse.Damage = data.damage;

        //pelletCount = 8;
        //spreadAngle = 45f;

        RealeasedBullet(spawnPoint);
    }

    private void RealeasedBullet(Transform spawnPoint)
    {
        if(unlockedLegado == false)
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
        else
        {
            //currentCoachCharge += Time.deltaTime;

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

    private void UpdateDefeteadEnemies(OnCoachDetectedDeadEnemy updateEvent)
    {
        currentEnemiesDefetead += updateEvent.point;
    }

    private void UpdateCurrentBullet(OnUnlockCoachLegado unlockEvent)
    {
        currentBulletUse = legadoBulletData;
    }
}
