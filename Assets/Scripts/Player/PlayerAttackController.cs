using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackController : MonoBehaviour
{
    [Header("Bullet Spawn")]
    [SerializeField] private Transform spawnPoint;

    [Header("Weapon")]
    [SerializeField] private GameObject weaponItem;
    private IWeapons weapon;

    private BulletPool pool;
    private LookObjectToMouse lookToMouseController;

    private float waitToFire;

    private TrainData dataRef;

    private InputAction repairAction;
    private bool isAttacking = false;

    private float currentReloadTime = 0;

    void Start()
    {
        //se busca la pool de objetos
        repairAction = InputSystem.actions.FindAction("Attack");
        repairAction.performed += ActiveAttack;
        repairAction.canceled += DeactiveAttack;

        lookToMouseController = GetComponent<LookObjectToMouse>();
        pool = GameObject.FindGameObjectWithTag("Factory").GetComponent<BulletPool>();

        //Se establece el arma equipada
        if (GameManager.Instance.Session.PlayerData.CheckWeapon() == false)
        {
            SetWeapon(weaponItem);
        }
        else
        {
            SetWeapon(GameManager.Instance.Session.PlayerData.PlayerWeapon);
        }

        waitToFire = weapon.RateOfFire;
        GameEvents.AmmoChanged(weapon.CurrentAmmunition);
    }

    void Update()
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
        spawnPoint.forward = lookToMouseController.GetMouseDirection(spawnPoint);
    }

    //Funcion utiliza por el Player Inputs para atacar 
    void Attack()
    {
        if (waitToFire > weapon.RateOfFire)
        {
            if (weapon.IsReloading) return;

            weapon.Shoot(spawnPoint);
            GameEvents.ShootPerformed();
            GameEvents.AmmoChanged(weapon.CurrentAmmunition);
            waitToFire = 0;
        }
    }

    void ActiveAttack (InputAction.CallbackContext context)
    {
        isAttacking = true;
    }

    void DeactiveAttack(InputAction.CallbackContext context)
    {
        isAttacking = false;
    }

    private void ChargeTimers()
    {
        if (waitToFire <= weapon.RateOfFire)
        {
            waitToFire += Time.deltaTime;
        }

        if (weapon.IsReloading)
        {
            currentReloadTime += Time.deltaTime;

            if(currentReloadTime > weapon.ReloadDuration)
            {
                currentReloadTime = 0;
                weapon.RestockBullets();
                weapon.IsReloading = false;
                GameEvents.AmmoChanged(weapon.CurrentAmmunition);
            }
        }
        
    }

    //Funcion para setear el arma equipada
    public void SetWeapon(GameObject weaponObtein)
    {
        weaponItem = weaponObtein;
        GameManager.Instance.Session.PlayerData.ChangeWeaponData(weaponItem);
        weapon = weaponItem.GetComponent<IWeapons>();
        weapon.RestockBullets();
        weapon.SetPool(pool);
        GameEvents.AmmoChanged(weapon.CurrentAmmunition);
    }
}
