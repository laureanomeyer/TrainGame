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
    private float currentHealth;
    private DamageFlash flash;

    private TrainRanges trainRanges;
    private (float, float) limits;

    public IEnemyWeapon Weapon;
    public IEnemyMovement Movement => data.movement;
    public IEnemyAttack Attack => data.attack;
    public IEnemyBrain Brain => data.brain;
    public Rigidbody rb;
    public float Speed => data.speed;
    public float MaxHealth => data.health;
    public float Damage => data.damage;
    public float Cooldown => data.attackCooldown;

    public List<IWagon> TargetList => targetList;
    public Transform Target => target;
    public float Range => data.range;

    public (float, float) Limits => limits;
    public bool moveRight;

    public bool CanAttack => attackCooldownTimer <= 0f;
    float attackCooldownTimer;

    public void Initialize(EnemyData data)
    {
        moveRight = false;

        this.data = data;
        currentHealth = MaxHealth;
        weaponPosition = GetComponentInChildren<Transform>();
        var WeaponGO = Instantiate(weapon, weaponPosition);
        Weapon = WeaponGO.GetComponent<EnemyWeapon>();
        rb = GetComponent<Rigidbody>();
        Brain.Begin(this);
        flash = GetComponent<DamageFlash>();
        GetComponent<Renderer>().materials = data.material;
        flash.SetMaterialArray(data.material);

        trainRanges = new();
        limits = trainRanges.SetRanges(Range, Vector3.zero);

    }

    public void ResetAttackCooldown(float cooldown)
    {
        attackCooldownTimer = cooldown;
    }

    void Update()
    {
        attackCooldownTimer -= Time.deltaTime;
        Attack?.Attack(this);
    }

    void FixedUpdate()
    {
        Movement?.Move(this);
    }

    public void SetTargetList(List<IWagon> targetList)
    {
        this.targetList = targetList;
        this.target = Brain.SetTarget(this);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth > 0)
        {
            flash.Flash();
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