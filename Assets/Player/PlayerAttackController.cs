using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackController : MonoBehaviour
{
    [Header("Bullet Spawn")]
    [SerializeField] Transform spawnPoint;

    [Header("Weapon")]
    [SerializeField] GameObject weaponItem;
    private IWeapons weapon;

    void Start()
    {
        weapon = weaponItem.GetComponent<IWeapons>();
    }

    void Update()
    {
        
    }

    void OnAttack(InputValue value)
    {
        Debug.Log("Attack");
        Attack();
    }

    private void Attack()
    {
        if (weapon != null)
        {
            weapon.Shoot(spawnPoint);
        }
    }

    public void SetWeapon(GameObject weaponObtein)
    {
        weaponItem = weaponObtein;
        weapon = weaponItem.GetComponent<IWeapons>();
    } 
}
