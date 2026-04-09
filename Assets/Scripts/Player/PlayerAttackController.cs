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

    void Start()
    {
        //se busca la pool de objetos
        lookToMouseController = GetComponent<LookObjectToMouse>();
        pool = GameObject.FindGameObjectWithTag("Factory").GetComponent<BulletPool>();

        //Se establece el arma equipada
        if (GameManager.Instance.PlayerData.CheckWeapon() == false)
        {
            SetWeapon(weaponItem);
            Debug.Log("Crea");
        }
        else
        {
            SetWeapon(GameManager.Instance.PlayerData.PlayerWeapon);
            Debug.Log("Carga");
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
        spawnPoint.forward = lookToMouseController.GetMouseDirection(spawnPoint);
    }

    //Funcion utiliza por el Player Inputs para atacar 
    void OnAttack(InputValue value)
    {
        if (waitToFire > weapon.RateOfFire)
        {
            if (weapon.CurrentAmmunition <= 0)
            {
                CallReload();
            }
            else
            {
                Attack();
                waitToFire = 0;
            }
        }
    }

    //Llama a la funcion de ataque del arma
    private void Attack()
    {
        //Verifica si se posee una arma equipada
        if (weapon != null)
        {
            weapon.Shoot(spawnPoint);
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
        weapon.Reload();
        weapon.SetPool(pool);
    }

    private void CallReload()
    {
        StartCoroutine(ActivateReload(weapon.ReloadDuration));
    }

    IEnumerator ActivateReload(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        weapon.Reload();
    }
}
