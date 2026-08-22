using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackController 
{
    public Transform spawnPoint;

    private GameObject weaponItem;
    private IWeapons weapon;

    private BulletPool pool;
    private LookObjectToMouse lookToMouseController;
    private PlayerBrain brain;
    private PlayerData playerDataRef;

    private bool isAttacking = false;
    public bool IsAttacking { get => isAttacking; }

    public PlayerAttackController(Transform spawnPoint, GameObject weaponItem, BulletPool pool, PlayerBrain brain, LookObjectToMouse look)
    {
        this.brain = brain;
        lookToMouseController = look;
        this.pool = pool;
        this.spawnPoint = spawnPoint;

        playerDataRef = ServiceLocator.Get<PlayerData>();

        //Se establece el arma equipada
        if (playerDataRef.CheckWeapon() == false)
        {
            SetWeapon(weaponItem);
        }
        else
        {
            SetWeapon(playerDataRef.PlayerWeapon);
        }

        EventBus.Publish(new OnAmmoChangedEvent(weapon.CurrentAmmunition));
    }

    public void Update()
    {
        AidToMouseDirection();

        if(weaponItem != null)
        {
            weapon.Tick(Time.deltaTime);
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

    public void ActiveAttack ()
    {
        isAttacking = true;
    }

    public void DeactiveAttack()
    {
        isAttacking = false;
    }

    //Funcion para setear el arma equipada
    public void SetWeapon(GameObject weaponObtein)
    {
        weaponItem = weaponObtein;
        playerDataRef.ChangeWeaponData(weaponItem);
        weapon = weaponItem.GetComponent<IWeapons>();
        weapon.InitializeWeapon(pool, this);
        weapon.RestockBullets();
        EventBus.Publish(new OnAmmoChangedEvent(weapon.CurrentAmmunition));
    }

    public void DestroyWeapon()
    {
        weapon.DestroyWeapon();
    }
}
