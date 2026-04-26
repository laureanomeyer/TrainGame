using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] EnemyData data;
    [SerializeField] GameObject weapon;
    [SerializeField] float CurrentH;
    private DamageFlash flash;

    private List<IWagon> targetList;
    private Transform target;
    private Transform weaponPosition;
    private float limitZ;


    public IEnemyWeapon Weapon;
    public IEnemyMovement Movement => data.movement;
    public IEnemyAttack Attack => data.attack;
    public IEnemyBrain Brain => data.brain;
    public float Speed => data.speed;
    public float MaxHealth => data.health;
    public float Damage => data.damage;
    public float Cooldown => data.attackCooldown;

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
        limitZ = Movement.SetLimitZ();
        flash = GetComponent<DamageFlash>();
    }

    public void ResetAttackCooldown(float cooldown)
    {
        attackCooldownTimer = cooldown;
    }

    void Update()
    {
        attackCooldownTimer -= Time.deltaTime;
        CurrentH = currentHealth;
        Movement?.Move(this, limitZ);
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
        Debug.Log("took: " + damage);
        if (currentHealth > 0)
            flash.Flash();
        else
            Dead();
    }

    private void Dead()
    {
        GameEvents.GoldEarned(data.gold);
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
            IBullet bullet = other.GetComponent<IBullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.Damage);
                bullet.Deactivate();
            }
        }

    }


}