using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] EnemyData data;
    [SerializeField] GameObject weapon;

    private List<IWagon> targetList;
    private Transform target;
    private Transform weaponPosition;

    public IEnemyWeapon Weapon;
    public IEnemyMovement Movement => data.movement;
    public IEnemyAttack Attack => data.attack;
    public IEnemyBrain Brain => data.brain;
    public float Speed => data.speed;
    public float MaxHealth => data.health;

    public List<IWagon> TargetList => targetList;

    public Transform Target => target;

    public float Range => data.range;

    private float currentHealth;


    void Awake()
    {
        currentHealth = MaxHealth;
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

    public void SetTargetList(List<IWagon> targetList)
    {
        this.targetList = targetList;
        this.target = Brain.SetTarget(this);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }
    //---------------------GIZMOS-------------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Range);

        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.transform.position);
        }
    }

}