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

    void Start()
    {
        //se busca la pool de objetos
        pool = GameObject.FindGameObjectWithTag("Factory").GetComponent<BulletPool>();

        //Se establece el arma equipada
        SetWeapon(weaponItem);
        weapon.SetPool(pool);
    }

    void Update()
    {
        
    }

    //Funcion utiliza por el Player Inputs para atacar 
    void OnAttack(InputValue value)
    {
        Attack();
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

    //Funcion para setear el arma equipada
    public void SetWeapon(GameObject weaponObtein)
    {
        weaponItem = weaponObtein;
        weapon = weaponItem.GetComponent<IWeapons>();
        weapon.SetPool(pool);
    } 
}
