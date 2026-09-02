using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] GameObject weapon;
    [SerializeField] SkinnedMeshRenderer enemyRend;
    [SerializeField] SkinnedMeshRenderer horseRend;
    [SerializeField] private EnemyUIHpBar healthBar;

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

    private bool isOnScreen;
    public bool IsOnScreen => isOnScreen;

    public EnemySkillSO Skill => data.skill;

    public Camera Cam => Camera.main;

    public void Initialize(EnemyData data)
    {
        StopAllCoroutines();

        moveRight = false;

        this.data = data;
        currentHealth = MaxHealth;
        weaponPosition = GetComponentInChildren<Transform>();
        var WeaponGO = Instantiate(weapon, weaponPosition);
        Weapon = WeaponGO.GetComponent<EnemyWeapon>();
        rb = GetComponent<Rigidbody>();

        Brain.Begin(this);

        if (enemyRend) enemyRend.sharedMesh = data.enemyMesh.sharedMesh;
        if (horseRend) horseRend.sharedMesh = data.horseMesh.sharedMesh;
        if (healthBar) healthBar.SetHealth(currentHealth, MaxHealth);

        flash = GetComponent<DamageFlash>();
        flash.StopCoroutine();
        flash.SetMaterialArray(0, data.material);

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


    public bool TakeDamage(float damage)
    {
        currentHealth -= damage;
        EventBus.Publish(new OnEnemyHitEvent(transform.position));
        flash.Flash();

        DamagePopupManager.Instance?.ShowDamage(
        damage,
        transform.position
    );


        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, MaxHealth);
        }
        if (currentHealth <= 0)
            Dead();

        return currentHealth <= 0;
    }

    private void Dead()
    {
        if (healthBar != null)
        { healthBar.Hide(); }
        flash.ResetMaterials();
        EventBus.Publish(new OnGoldEarnedEvent(data.gold));
        EventBus.Publish(new OnEnemyDeathEvent(transform.position));
        EventBus.Publish(new OnEnemyKilledEvent());
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    private void DeadWallDeath()
    {
        if (healthBar != null)
        { healthBar.Hide(); }
        ObjectPoolManager.ReturnObjectToPool(gameObject);
        flash.ResetMaterials();
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