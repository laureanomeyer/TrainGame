using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] EnemyData data;
    [SerializeField] GameObject weapon;

    private Transform target;
    private Transform weaponPosition;

    public IEnemyWeapon Weapon;
    public IEnemyMovement Movement => data.movement;
    public IEnemyAttack Attack => data.attack;
    public IEnemyBrain Brain => data.brain;
    public float Speed => data.speed;

    public Transform Target => target;

    public float Range => data.range;




    void Awake()
    {
        weaponPosition = GetComponentInChildren<Transform>();
        var WeaponGO = Instantiate(weapon, weaponPosition);
        Weapon = WeaponGO.GetComponent<EnemyWeapon>();
        Brain.Begin(this);
    }

    private void Update()
    {
        if (Movement != null) 
        {
            Movement.Move(this);
        }

        Attack.Attack(this);

    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

}