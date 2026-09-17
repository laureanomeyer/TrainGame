using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] GameObject weapon;
    [Header("Meshes")]
    [SerializeField] SkinnedMeshRenderer enemyRend;
    [SerializeField] SkinnedMeshRenderer horseRend;
    [Header("UI")]
    [SerializeField] private EnemyUIHpBar healthBar;
    [Header("Animators")]
    [SerializeField] Animator cowboyAnimator;
    [SerializeField] Animator horseAnimator;

    private EnemyData data;
    private List<IWagon> targetList;
    private IWagon targetWagon;
    private Transform target;
    private Transform weaponPosition;
    private float currentHealth;
    private DamageFlash flash;
    private bool isDead;
    private int activeCowboyLayer;
    private Coroutine attackRoutine;

    private TrainRanges trainRanges;
    private (float, float) limits;

    public IEnemyWeapon Weapon;
    public EnemyMovementSO Movement => data.movement;
    public EnemyAttackSO Attack => data.attack;
    public EnemyBrainSO Brain => data.brain;
    public Rigidbody rb;
    public float Speed => data.speed;
    public float MaxHealth => data.health;
    public float Damage => data.damage;
    public float Cooldown => data.attackCooldown;
    public DropType Drop => data.drop;
    public List<IWagon> TargetList => targetList;
    public Transform Target => target;
    public IWagon TargetWagon => targetWagon;
    public float Range => data.range;

    public (float, float) Limits => limits;

    float attackCooldownTimer;
    float skillCooldownTimer;

    public bool CanAttack => attackCooldownTimer <= 0f;
    public bool CanSkill => skillCooldownTimer <= 0f;

    public EnemySkillSO Skill => data.skill;

    private float spawnTime;
    public float TimeAlive => Time.time - spawnTime;
    public bool IsOnPositiveZSide => 
        rb != null && rb.position.z >= 0f;
    public bool IsOnNegativeZSide =>
        rb != null && rb.position.z < 0f;
    public Camera Cam => Camera.main;

    int rightLayerIndex;
    int leftLayerIndex;



    void Awake()
    {
        Weapon = GetComponentInChildren<EnemyWeapon>();

        rightLayerIndex = cowboyAnimator.GetLayerIndex("Right Layer");
        leftLayerIndex = cowboyAnimator.GetLayerIndex("Left Layer");
    }

    public void Initialize(EnemyData data)
    {
        StopAllCoroutines();

        isDead = false;
        this.data = data;
        currentHealth = MaxHealth;
        spawnTime = Time.time;
        rb = GetComponent<Rigidbody>();
        skillCooldownTimer = Skill.Cooldown;
        attackCooldownTimer = data.attackCooldown;

        Brain.Begin(this);

        if (enemyRend) enemyRend.sharedMesh = data.enemyMesh.sharedMesh;
        if (horseRend) horseRend.sharedMesh = data.horseMesh.sharedMesh;
        if (healthBar) healthBar.SetHealth(currentHealth, MaxHealth);

        PlayIdleAnimation();

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

    public void ResetSkillCooldown(float cooldown)
    {
        skillCooldownTimer = cooldown;
    }

    void Update()
    {
        if (isDead) return;

        attackCooldownTimer -= Time.deltaTime;
        skillCooldownTimer -= Time.deltaTime;
        Attack?.Attack(this);
        Attack?.Skill(this);
    }

    void FixedUpdate()
    {
        if (isDead) return;

        Movement?.Move(this);
    }

    public void SetTargetList(List<IWagon> targetList)
    {
        this.targetList = targetList;
        this.target = Brain.SetTarget(this);

        targetWagon = null;
        for (int i = 0; i < targetList.Count; i++)
        {
            IWagon wagon = targetList[i];
            if (wagon.Head == target || wagon.Tail == target)
            {
                targetWagon = wagon;
                break;
            }
        }
    }

    public void PlayIdleAnimation()
    {
        PlayCowboyAnimation(GetAnimationName(
            IsOnPositiveZSide ? "Cowboy_1|L_Idle" : "Cowboy_1|R_Idle",
            IsOnPositiveZSide ? data.animation?.positiveZIdle : data.animation?.negativeZIdle));

        PlayHorseAnimation(data.animation?.horseIdle ?? "Horse|Idle");
    }

    public void PlayAttackAnimation()
    {
        PlayCowboyAnimation(GetAnimationName(
            IsOnPositiveZSide ? "Cowboy_1|L_Aim" : "Cowboy_1|R_Aim 0",
            IsOnPositiveZSide ? data.animation?.positiveZAttack : data.animation?.negativeZAttack));
    }

    private string GetAnimationName(string defaultName, string configuredName)
    {
        return string.IsNullOrEmpty(configuredName) ? defaultName : configuredName;
    }

    private System.Collections.IEnumerator ReturnToIdleAfterAttack()
    {
        yield return new WaitForSeconds(data.animation?.attackDuration ?? 0.8f);

        if (!isDead) PlayIdleAnimation();
        attackRoutine = null;
    }

    private void PlayCowboyAnimation(string stateName)
    {
        if (string.IsNullOrEmpty(stateName) || cowboyAnimator == null) return;

        activeCowboyLayer = cowboyAnimator.GetLayerIndex(IsOnPositiveZSide ? "Left Layer" : "Right Layer");

        if (activeCowboyLayer < 0) return;

        cowboyAnimator.SetLayerWeight(activeCowboyLayer, 1f);
        cowboyAnimator.Play(stateName, activeCowboyLayer, 0f);
    }

    private void PlayHorseAnimation(string stateName)
    {
        if (string.IsNullOrEmpty(stateName) || horseAnimator == null) return;

        horseAnimator.Play(stateName, 0, 0f);
    }

    public bool TakeDamage(float damage)
    {
        if (isDead) return false;
        
        currentHealth -= damage;
        EventBus.Publish(new OnEnemyHitEvent(transform.position));
        
        
        flash.Flash();
        DamagePopupManager.Instance?.ShowDamage(damage,transform.position);
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, MaxHealth);
        }
        if (currentHealth <= 0)
            Dead();

        return currentHealth <= 0;
    }
    public void OnAttackAnimationFinished()
    {
        if (isDead) return;

        PlayIdleAnimation();
        attackRoutine = null;
    }
    private void Dead()
    {
        if (isDead) return;

        isDead = true;
        PlayCowboyAnimation(GetAnimationName(
            IsOnPositiveZSide ? "Cowboy_1|L_Death" : "Cowboy_1|R_Death",
            IsOnPositiveZSide
                ? data.animation?.positiveZDeath
                : data.animation?.negativeZDeath));

        if (healthBar != null)
        { healthBar.Hide(); }
        flash.ResetMaterials();
        if (Drop == DropType.Coal)
        {
            EventBus.Publish(new OnCoalEarnedEvent(data.dropAmount));
        }
        else if (Drop == DropType.Gold)
        {
            EventBus.Publish(new OnGoldEarnedEvent(data.dropAmount));
        }
        EventBus.Publish(new OnEnemyDeathEvent(transform.position, data.drop));
        EventBus.Publish(new OnEnemyKilledEvent());
        StartCoroutine(ReturnAfterDeathAnimation());
    }

    private System.Collections.IEnumerator ReturnAfterDeathAnimation()
    {
        yield return new WaitForSeconds(data.animation?.deathDuration ?? 1f);
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    private void DeadWallDeath()
    {
        isDead = true;
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