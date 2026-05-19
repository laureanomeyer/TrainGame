using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] GameObject weapon;

    private EnemyData data;
    private List<IWagon> targetList;
    private Transform target;
    private Transform weaponPosition;
    private Rigidbody rb;
    private float limitZ;
    private float currentHealth;
    private DamageFlash flash;

    public Vector3 KnockbackVelocity = Vector3.zero; //temporal
    public bool IsKnocked = false; //temporal

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
    //Vector3 KnockbackVelocity => knockbackVelocity;


    public bool CanAttack => attackCooldownTimer <= 0f;
    float attackCooldownTimer;

    public void Initialize(EnemyData data)
    {
        this.data = data;
        currentHealth = MaxHealth;
        weaponPosition = GetComponentInChildren<Transform>();
        var WeaponGO = Instantiate(weapon, weaponPosition);
        Weapon = WeaponGO.GetComponent<EnemyWeapon>();
        rb = GetComponent<Rigidbody>();
        Brain.Begin(this);
        flash = GetComponent<DamageFlash>();
        limitZ = Movement.SetLimitZ();
        GetComponent<Renderer>().materials = data.material;
        flash.SetMaterialArray(data.material);
    }

    public void ResetAttackCooldown(float cooldown)
    {
        attackCooldownTimer = cooldown;
    }

    void Update()
    {
        attackCooldownTimer -= Time.deltaTime;
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
        //Debug.Log("took: " + damage);
        if (currentHealth > 0)
        {
            flash.Flash();

            IsKnocked = true;

            KnockbackVelocity += -transform.forward * 6f;
        }

        else
            Dead();
    }

    private void Dead()
    {
        GameEvents.GoldEarned(data.gold);
        GameEvents.EnemyDeath(transform.position);
        Destroy(gameObject);
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
    }
}