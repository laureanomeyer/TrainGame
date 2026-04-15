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

    void Start()
    {
        //se busca la pool de objetos
        repairAction = InputSystem.actions.FindAction("Attack");
        repairAction.performed += Attack;

        lookToMouseController = GetComponent<LookObjectToMouse>();
        pool = GameObject.FindGameObjectWithTag("Factory").GetComponent<BulletPool>();

        //Se establece el arma equipada
        if (GameManager.Instance.PlayerData.CheckWeapon() == false)
        {
            SetWeapon(weaponItem);
        }
        else
        {
            SetWeapon(GameManager.Instance.PlayerData.PlayerWeapon);
        }

        waitToFire = weapon.RateOfFire;
    }

    void Update()
    {
        AidToMouseDirection();
        ChargeTimers();
    }

    private void AidToMouseDirection()
    {
        if (lookToMouseController == null) return;
        spawnPoint.forward = lookToMouseController.GetMouseDirection(spawnPoint);
    }

    //Funcion utiliza por el Player Inputs para atacar 
    void Attack(InputAction.CallbackContext context)
    {
        if (waitToFire > weapon.RateOfFire)
        {
            if (weapon.CurrentAmmunition <= 0)
            {
                CallReload();
            }
            else
            {
                weapon.Shoot(spawnPoint);
                waitToFire = 0;
            }
        }
    }

    private void ChargeTimers()
    {
        if (waitToFire <= weapon.RateOfFire)
        {
            waitToFire += Time.deltaTime;
        }

        
    }

    //Funcion para setear el arma equipada
    public void SetWeapon(GameObject weaponObtein)
    {
        weaponItem = weaponObtein;
        GameManager.Instance.PlayerData.ChangeWeaponData(weaponItem);
        weapon = weaponItem.GetComponent<IWeapons>();
        weapon.RestockBullets();
        weapon.SetPool(pool);
    }

    private void CallReload()
    {
        StartCoroutine(ActivateReload(weapon.ReloadDuration));
    }

    IEnumerator ActivateReload(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        weapon.RestockBullets();
    }
}
