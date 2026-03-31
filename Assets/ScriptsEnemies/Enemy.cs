using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] EnemyData data;
    [SerializeField] GameObject weapon;
    [SerializeField] float CurrentH;

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

    public bool CanAttack => attackCooldownTimer <= 0f;
    float attackCooldownTimer;

    void Awake()
    {
        currentHealth = MaxHealth;
        weaponPosition = GetComponentInChildren<Transform>();
        var WeaponGO = Instantiate(weapon, weaponPosition);
        Weapon = WeaponGO.GetComponent<EnemyWeapon>();
        Brain.Begin(this);
    }

    public void ResetAttackCooldown(float cooldown)
    {
        attackCooldownTimer = cooldown;
    }

    void Update()
    {
        attackCooldownTimer -= Time.deltaTime;
        CurrentH = currentHealth;
        Movement?.Move(this);
        Attack?.Attack(this);
    }

    public void SetTargetList(List<IWagon> targetList)
    {
        this.targetList = targetList;
        this.target = Brain.SetTarget(this);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Dead();
    }

    private void Dead()
    {
        Destroy(this.gameObject);
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
    //---------------------TRIGGER----------------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("deadWall"))
        {
            Dead();
        }
        if (other.gameObject.CompareTag("bullet"))
        {
            int damage = other.GetComponent<BulletScript>().Damage;
            TakeDamage(damage);
            Debug.Log(damage);
        }

    }


}