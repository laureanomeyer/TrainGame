using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackController 
{
    private Transform spawnPoint;

    private GameObject weaponItem;
    private IWeapons weapon;

    private BulletPool pool;
    private LookObjectToMouse lookToMouseController;
    private PlayerBrain brain;

    private float waitToFire = 0;

    private bool isAttacking = false;

    private float currentReloadTime = 0;

    private float rateOfFire;
    public float RateOfFire { get => rateOfFire; }

    private float reloadTime;
    public float ReloadTime { get => reloadTime; }

    public PlayerAttackController(Transform spawnPoint, GameObject weaponItem, BulletPool pool, PlayerBrain brain, LookObjectToMouse look)
    {
        this.brain = brain;
        lookToMouseController = look;
        this.pool = pool;
        this.spawnPoint = spawnPoint;

        //Se establece el arma equipada
        if (GameManager.Instance.Session.PlayerData.CheckWeapon() == false)
        {
            SetWeapon(weaponItem);
        }
        else
        {
            SetWeapon(GameManager.Instance.Session.PlayerData.PlayerWeapon);
        }

        GameEvents.AmmoChanged(weapon.CurrentAmmunition);
    }

    public void Update()
    {
        AidToMouseDirection();
        ChargeTimers();

        if (isAttacking)
        {
            Attack();
        }
    }

    private void AidToMouseDirection()
    {
        if (lookToMouseController == null) return;
        if (spawnPoint == null) return;
        Vector3 dir = lookToMouseController.GetMouseDirection(spawnPoint);
        dir.y = 0;
        spawnPoint.forward = dir;
    }

    void Attack()
    {
        if (waitToFire > rateOfFire)
        {
            if (weapon.IsReloading) return;

            weapon.Shoot(spawnPoint);
            GameEvents.ShootPerformed();
            GameEvents.AmmoChanged(weapon.CurrentAmmunition);
            waitToFire = 0;
        }
    }

    public void ActiveAttack ()
    {
        isAttacking = true;
    }

    public void DeactiveAttack()
    {
        isAttacking = false;
    }

    private void ChargeTimers()
    {
        if (waitToFire <= rateOfFire)
        {
            waitToFire += Time.deltaTime;
        }

        if (weapon.IsReloading)
        {
            currentReloadTime += Time.deltaTime;

            if(currentReloadTime > reloadTime)
            {
                currentReloadTime = 0;
                weapon.RestockBullets();
                weapon.IsReloading = false;
                GameEvents.AmmoChanged(weapon.CurrentAmmunition);
            }
        }
        
    }

    public void ReseatCadenceStats()
    {
        rateOfFire = weapon.WeaponData.rateOfFire / GameManager.Instance.Session.StatSystem.GetStat(StatType.AttackSpeed);
        reloadTime = weapon.WeaponData.reloadTime / GameManager.Instance.Session.StatSystem.GetStat(StatType.AttackSpeed); ;
    }

    //Funcion para setear el arma equipada
    public void SetWeapon(GameObject weaponObtein)
    {
        weaponItem = weaponObtein;
        GameManager.Instance.Session.PlayerData.ChangeWeaponData(weaponItem);
        weapon = weaponItem.GetComponent<IWeapons>();
        ReseatCadenceStats();
        weapon.RestockBullets();
        weapon.SetPool(pool);
        GameEvents.AmmoChanged(weapon.CurrentAmmunition);
    }
}
