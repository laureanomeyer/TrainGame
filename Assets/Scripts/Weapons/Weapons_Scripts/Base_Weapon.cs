
using UnityEngine;

public class Base_Weapon : MonoBehaviour, IWeapons, IWeaponBuffer
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

    public Transform weaponSpawnPoint;

    private int currentAmmunition;
    public int CurrentAmmunition { get => currentAmmunition; set => currentAmmunition = value; }

    private bool isReloading =false;
    public bool IsReloading { get => isReloading; set => isReloading = value; }


    //Rate of Fire
    private float waitToFire = 0;

    private float baseRateOfFire;
    private float rateOfFire;
    public float RateOfFire { get => rateOfFire; }
    private float rateOfFireBuff;


    //Reloding time
    private float currentReloadTime = 0;

    private float baseReloadTime;
    private float reloadTime;
    public float ReloadTime { get => reloadTime; }

    //Damage
    private float baseDamage;
    private float damage;
    private float damageBuff;

    //Referencia a la pool de balas
    private BulletPool bulletPool;
    public BulletPool BulletPool => bulletPool;

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

        EventBus.Subscribe<OnStatChangedEvent>(UpdateRoFStats);
        EventBus.Publish(new OnAmmoChangedEvent(currentAmmunition));
    }

    public void DestroyWeapon()
    {
        EventBus.Unsubscribe<OnStatChangedEvent>(UpdateRoFStats);
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

    public void Shoot()
    {
        if (IsReloading) return;
        if (weaponSpawnPoint == null) return;

        var data = WeaponData;
        bulletData.Damage = data.damage;


        BulletPool.ShootObject(weaponSpawnPoint.position, weaponSpawnPoint.rotation, bulletData);

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
        //EventBus.Publish(new OnShootEvent(rateOfFire));
        EventBus.Publish(new OnAmmoChangedEvent(currentAmmunition));
        waitToFire = 0;
    }

    public void RestockWeapon()
    {
        currentReloadTime = 0;
        RestockBullets();
        IsReloading = false;
        AudioManager.Instance.Play($"SFXDefaultShot");
        EventBus.Publish(new OnAmmoChangedEvent(currentAmmunition));
    }

    public void UpdateRoFStats(OnStatChangedEvent @event)
    {
        var statsRef = ServiceLocator.Get<StatSystem>();

        //Rate of fire set up
        rateOfFire = WeaponData.rateOfFire / statsRef.GetStat(StatType.AttackSpeed);
        baseRateOfFire = rateOfFire;

        if (rateOfFireBuff > 0)
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
    }

    public void BufferRoF(float buffer)
    {
        rateOfFireBuff = buffer;
        rateOfFire = rateOfFire * (1f - rateOfFireBuff / 100f);
        Debug.Log("Weapon buff updated " + gameObject.name + "; Weapon ROF: " + rateOfFire);
    }

    public void DebuffRoF()
    {
        rateOfFireBuff = 0;
        rateOfFire = baseRateOfFire;
        Debug.Log("Weapon debuff updated " + gameObject.name + "; Weapon ROF: " + rateOfFire);
    }

    public void BufferDamage(float buffer)
    {
        damageBuff = buffer;
        damage = damage * (1f + damageBuff / 100f);
        Debug.Log("Weapon buff Damage updated " + gameObject.name + "; Weapon Damage: " + damage);
    }

    public void DebuffDamage()
    {
        damageBuff = 0;
        damage = baseDamage;
        Debug.Log("Weapon debuff Damage updated " + gameObject.name + "; Weapon damage: " + damage);
    }
}
