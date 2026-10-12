using System.Collections;
using UnityEngine;

public class Colt_Weapon : MonoBehaviour, IWeapons, IWeaponBuffer
{
    [Header("Name")]
    [SerializeField] private string weaponName;
    public string Id => weaponName;

    private PlayerAttackController playerAtkReference;

    [Header("Weapon data")]
    [SerializeField] private WeaponDataSO weaponData;

    [Header("Particle Sequence Controller")]
    [SerializeField] ParticleSequenceController sequenceController;

    public Transform leftWeaponSpawnPoint;
    public Transform rightWeaponSpawnPoint;

    [Header("Bullet data")]
    [SerializeField] private BulletTypeScriptable bulletData;
    [SerializeField] private BulletTypeScriptable legacyBulletData;
    public WeaponDataSO WeaponData { get => weaponData; set => weaponData = value; }

    private int currentAmmunition;
    public int CurrentAmmunition { get => currentAmmunition; set => currentAmmunition = value; }

    private bool isReloading = false;
    public bool IsReloading { get => isReloading; set => isReloading = value; }

    //Rate of Fire
    private float waitToFire = 0;

    private float baseRateOfFire;

    private float rateOfFire;
    public float RateOfFire { get => rateOfFire; }

    private float rateOfFireBuff = 0;

    //Reload time
    private float currentReloadTime = 0;

    private float reloadTime;
    public float ReloadTime { get => reloadTime; }

    //Damage
    private float baseDamage;
    private float damage;
    private float damageBuff = 0;

    [Header("Delay between shoots")]
    [SerializeField] private float delayBetweenShots = 0.05f; // segundos entre bala 1 y bala 2
    private bool isShooting;

    //Referencia a la pool de balas
    private BulletPool bulletPool;
    public BulletPool BulletPool => bulletPool;

    [Header("Bullet spread")]
    [SerializeField] private float bulletSpreadAngle = 10f;

    private bool unlockedLegacy = false;

    [Header("Legacy unlock condition")]
    [SerializeField] private int unlockLegacyCondition = 1;
    private int curretEnemiesDefetead = 0;

    public void InitializeWeapon(BulletPool pool, PlayerAttackController playerAttack)
    {
        //Bullet pool set up
        bulletPool = pool;
        playerAtkReference = playerAttack;

        var statsRef = ServiceLocator.Get<StatSystem>();

        //Rate of fire set up
        rateOfFire = WeaponData.rateOfFire / statsRef.GetStat(StatType.AttackSpeed);
        baseRateOfFire = rateOfFire;

        //Reload time set up
        reloadTime = WeaponData.reloadTime / statsRef.GetStat(StatType.AttackSpeed);

        //Damage set up
        damage = WeaponData.damage * statsRef.GetStat(StatType.DamageMultiplier);
        baseDamage = damage;

        EventBus.Subscribe<OnColtDetectedDeadEnemy>(UpdateDefeteadEnemies);
        EventBus.Subscribe<OnUnlockColtLegado>(UpdateCurrentBullet);
        EventBus.Subscribe<OnStatChangedEvent>(UpdateStats);

        PlayerData playerData = ServiceLocator.Get<PlayerData>();

        if (playerData.unlockedLegado.UnlockedCoach)
        {
            reloadTime = 0f;
            unlockedLegacy = true;
            sequenceController.PlayGroup(ParticleGroups.LegacyUnlocked);
        }

        EventBus.Publish(new OnAmmoChangedEvent(currentAmmunition));
    }

    public void DestroyWeapon()
    {
        EventBus.Unsubscribe<OnColtDetectedDeadEnemy>(UpdateDefeteadEnemies);
        EventBus.Unsubscribe<OnUnlockColtLegado>(UpdateCurrentBullet);
        EventBus.Unsubscribe<OnStatChangedEvent>(UpdateStats);
        Debug.Log("Desuscribi evento " + gameObject.name);
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
        if (isShooting || IsReloading) return;

        if (waitToFire > rateOfFire)
        {
            Shoot();
            EventBus.Publish(new OnShootEvent(rateOfFire));
            // ya NO reiniciamos waitToFire aquí
        }
    }

    public void Shoot()
    {
        if (isShooting) return;   // ya hay una ráfaga en curso
        if (IsReloading) return;
        if (rightWeaponSpawnPoint == null || leftWeaponSpawnPoint == null) return;

        if (unlockedLegacy == false)
        {
            if (curretEnemiesDefetead >= unlockLegacyCondition)
            {
                Debug.Log("Legado de colt desbloquado");
                EventBus.Publish(new OnUpdatedColtLegado());
            }
        }

        bulletData.Damage = damage;

        isShooting = true;        // se bloquea ANTES de iniciar la corrutina
        StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        // Primer disparo
        BulletPool.ShootObject(rightWeaponSpawnPoint.position, rightWeaponSpawnPoint.rotation, bulletData);
        sequenceController.Play("shotParticle1");
        CurrentAmmunition--;
        EventBus.Publish(new OnAmmoChangedEvent(CurrentAmmunition));

        yield return new WaitForSeconds(delayBetweenShots);

        // Segundo disparo
        if (CurrentAmmunition > 0)
        {
            BulletPool.ShootObject(leftWeaponSpawnPoint.position, leftWeaponSpawnPoint.rotation, bulletData);
            sequenceController.Play("shotParticle2");
            CurrentAmmunition--;
            EventBus.Publish(new OnAmmoChangedEvent(CurrentAmmunition));
        }

        waitToFire = 0;      // el cooldown empieza AHORA, al terminar la ráfaga
        isShooting = false;

        if (CurrentAmmunition <= 0)
        {
            if (unlockedLegacy == false && curretEnemiesDefetead < unlockLegacyCondition)
            {
                Debug.Log("Legado de colt no desbloquado");
                curretEnemiesDefetead = 0;
            }

            IsReloading = true;
            EventBus.Publish(new OnReloadEvent(reloadTime));
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isShooting = false;
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
        EventBus.Publish(new OnAmmoChangedEvent(currentAmmunition));
        waitToFire = 0;
    }

    public void RestockWeapon()
    {
        currentReloadTime = 0;
        RestockBullets();
        IsReloading = false;
        EventBus.Publish(new OnAmmoChangedEvent(currentAmmunition));
    }

    private void UpdateDefeteadEnemies(OnColtDetectedDeadEnemy updateEvent)
    {
        Debug.Log("Se derroto un enemigo de la colt");
        curretEnemiesDefetead += updateEvent.point;
    }

    private void UpdateCurrentBullet(OnUnlockColtLegado unlockEvent)
    {
        Debug.Log("Finalizado desbloqueo de colt");
        reloadTime = 0f;
        unlockedLegacy = true;
        sequenceController.PlayGroup(ParticleGroups.LegacyUnlocked);
    }

    public void UpdateStats(OnStatChangedEvent @event)
    {
        var statsRef = ServiceLocator.Get<StatSystem>();

        //Rate of fire set up
        rateOfFire = WeaponData.rateOfFire / statsRef.GetStat(StatType.AttackSpeed);
        baseRateOfFire = rateOfFire;

        if(rateOfFireBuff > 0)
        {
            rateOfFire = rateOfFire * (1f - rateOfFireBuff / 100f);
        }

        //Reload time set up
        reloadTime = WeaponData.reloadTime / statsRef.GetStat(StatType.AttackSpeed);

        //Damage set up
        damage = WeaponData.damage * statsRef.GetStat(StatType.DamageMultiplier);
        baseDamage = damage;

        if (damageBuff > 0)
        {
            damage = damage * (1f + damageBuff / 100f);
        }
        Debug.Log("Stats actualizadas");
    }

    public void BufferRoF(float buffer)
    {
        rateOfFireBuff += buffer;
        rateOfFire = baseRateOfFire * (1f - rateOfFireBuff / 100f);
        Debug.Log("Weapon buff updated " + gameObject.name + "; Weapon ROF: " + rateOfFire);
    }

    public void DebuffRoF(float buffer)
    {
        rateOfFireBuff -= buffer;
        if (rateOfFireBuff > 0)
        {
            rateOfFire = baseRateOfFire * (1f - rateOfFireBuff / 100f);
        }
        else
        {
            rateOfFire = baseRateOfFire;
        }
        Debug.Log("Weapon debuff updated " + gameObject.name + "; Weapon ROF: " + rateOfFire);
    }

    public void BufferDamage(float buffer)
    {
        damageBuff += buffer;
        damage = baseDamage * (1f + damageBuff / 100f);
        Debug.Log("Weapon buff Damage updated " + gameObject.name + "; Weapon Damage: " + damage);
    }

    public void DebuffDamage(float buffer)
    {
        damageBuff -= buffer;
        if (damageBuff > 0)
        {
            damage = baseDamage * (1f + damageBuff / 100f);
        }
        else
        {
            damage = baseDamage;
        }
        Debug.Log("Weapon debuff Damage updated " + gameObject.name + "; Weapon damage: " + damage);
    }
}
