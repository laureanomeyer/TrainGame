using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] GameObject weapon;
    [SerializeField] GameObject cowboyRender;
    [SerializeField] private EnemyUIHpBar healthBar;

    float CurrentH;

    private EnemyData data;
    private List<IWagon> targetList;
    private Transform target;
    private Transform weaponPosition;
    private float currentHealth;
    private DamageFlash flash;
    private MeshFilter filter;

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
        cowboyRender = data.SMR;
        flash = GetComponent<DamageFlash>();
        flash.SetMaterialArray(0, data.material);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, MaxHealth);
        }

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
        GameEvents.EnemyHit(transform.position);
        flash.Flash();

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, MaxHealth);
        }
        if (currentHealth <= 0)
            Dead();
    }

    private void Dead()
    {
        if (healthBar != null)
        { healthBar.Hide(); }
        GameEvents.GoldEarned(data.gold);
        GameEvents.EnemyDeath(transform.position);
        TutorialEvents.EnemyKilled();
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    private void DeadWallDeath()
    {
        if (healthBar != null)
        { healthBar.Hide(); }
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    //---------------------GIZMOS-------------------------

    void OnDrawGizmosSelected()
    {
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
            DeadWallDeath();
        }
    }
}