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
    [Header("Debug")]
    public string TargetWagonName;
    public string dataName;
    private EnemyData data;
    private IWagon targetWagon;
    private float currentHealth;
    private DamageFlash flash;
    private bool isDead;
    private int activeCowboyLayer;
    private Coroutine attackRoutine;
    private TrainRanges trainRanges;
    private (float, float) limits;
    public EnemyWeapon Weapon;
    public EnemyMovementSO Movement => data.movement;
    public EnemyAttackSO Attack => data.attack;
    public EnemyBrainSO Brain => data.brain;
    public Rigidbody rb;
    BoxCollider boxCollider;
    public float Speed => data.speed;
    public float MaxHealth => data.health;
    public float Damage => data.damage;
    public float Cooldown => data.attackCooldown;
    public DropType Drop => data.drop;
    public IWagon TargetWagon => targetWagon;
    public EnemyData Data => data;

    public float Range => data.range;
    public (float, float) Limits => limits;

    float attackCooldownTimer;
    float skillCooldownTimer;

    public bool CanAttack => attackCooldownTimer <= 0f;
    public bool CanSkill => skillCooldownTimer <= 0f;

    public EnemySkillSO Skill => data.skill;
    private float timeAtTarget;
    private bool isAtTarget;
    private bool isLosing;
    public float TimeAtTarget => isAtTarget ? Time.time - timeAtTarget : 0f;
    public bool IsLosing => isLosing;
    public bool IsOnPositiveZSide =>
        rb != null && rb.position.z >= 0f;
    public bool IsOnNegativeZSide =>
        rb != null && rb.position.z < 0f;
    public bool IsDead => isDead;
    public Camera Cam => Camera.main;

    public bool IsTutorialEnemy { get; private set; }

    int rightLayerIndex;
    int leftLayerIndex;
    void Awake()
    {
        Weapon = GetComponentInChildren<EnemyWeapon>();
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

        rightLayerIndex = cowboyAnimator.GetLayerIndex("Right Layer");
        leftLayerIndex = cowboyAnimator.GetLayerIndex("Left Layer");

        EventBus.Subscribe<OnSetTutorialEnemyTarget>(SetSingleTargetByEvent);
        EventBus.Subscribe<OnWagonDestroyedEvent>(RetargetWagon);
    }
    void OnDestroy()
    {
        EventBus.Unsubscribe<OnSetTutorialEnemyTarget>(SetSingleTargetByEvent);
        EventBus.Unsubscribe<OnWagonDestroyedEvent>(RetargetWagon);
    }

    public void Initialize(EnemyData data)
    {
        StopAllCoroutines();
        attackRoutine = null;

        isDead = false;
        isAtTarget = false;
        timeAtTarget = 0f;
        isLosing = false;
        this.data = data;
        currentHealth = MaxHealth;
        skillCooldownTimer = Skill.Cooldown;
        attackCooldownTimer = data.attackCooldown;

        dataName = data.name;

        if (enemyRend) enemyRend.sharedMesh = data.enemyMesh.sharedMesh;
        if (horseRend) horseRend.sharedMesh = data.horseMesh.sharedMesh;
        if (healthBar) healthBar.SetHealth(currentHealth, MaxHealth);

        PlayIdleAnimation();

        flash = GetComponent<DamageFlash>();
        flash.StopCoroutine();
        flash.SetMaterialArray(0, data.material);

        trainRanges = new();
        limits = trainRanges.SetRanges(Range, Vector3.zero);

        if (!IsTutorialEnemy)
        { 
            targetWagon = Brain.GetPreference(data.targetPreference);
        }
    }

    public void ResetAttackCooldown(float cooldown)
    {
        attackCooldownTimer = cooldown;
    }

    public void ResetSkillCooldown(float cooldown)
    {
        skillCooldownTimer = cooldown;
    }

    public void SetAtTarget(bool atTarget)
    {
        if (atTarget && !isAtTarget)
        {
            timeAtTarget = Time.time;
        }

        isAtTarget = atTarget;
    }

    public void BeginLosing()
    {
        isLosing = true;
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

    #region Events
    public void SetSingleTargetByEvent(OnSetTutorialEnemyTarget ev)
    {
        targetWagon = null;

        targetWagon = RunManager.Instance.ActiveWagons[ev.index];
    }

    public void RetargetWagon(OnWagonDestroyedEvent ev)
    {
        if (IsTutorialEnemy) return;

        if (targetWagon == ev.WagonInstance)
        {
            targetWagon = Brain.ReTarget(ev.WagonInstance);
        }
    }

    #endregion

    #region Animations

    public void PlayIdleAnimation()
    {
        PlayCowboyAnimation(GetAnimationName(
            IsOnPositiveZSide ? "Cowboy_1|L_Idle" : "Cowboy_1|R_Idle",
            IsOnPositiveZSide ? data.animation?.positiveZIdle : data.animation?.negativeZIdle));

        PlayHorseAnimation(data.animation?.horseIdle ?? "Horse|Idle");
    }

    public void PlayAttackAnimation()
    {
        if (attackRoutine != null) return;

        PlayCowboyAnimation(GetAnimationName(
            IsOnPositiveZSide ? "Cowboy_1|L_Aim" : "Cowboy_1|R_Aim 0",
            IsOnPositiveZSide ? data.animation?.positiveZAttack : data.animation?.negativeZAttack));

        attackRoutine = StartCoroutine(ReturnToIdleAfterAttack());
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
    private System.Collections.IEnumerator ReturnAfterDeathAnimation()
    {
        yield return new WaitForSeconds(data.animation?.deathDuration ?? 1f);
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }
    #endregion
    public bool TakeDamage(float damage)
    {
        if (isDead) return false;

        currentHealth -= damage;
        EventBus.Publish(new OnEnemyHitEvent(transform.position));


        flash.Flash();
        DamagePopupManager.Instance?.ShowDamage(damage, transform.position);
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
        if (isDead) return;

        isDead = true;
        PlayDeathSound();
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

    void PlayDeathSound()
    {
        int soundNumber = Random.Range(1, 1001) == 1000 ? 4 : Random.Range(1, 4);
        AudioManager.Instance.PlayOnScreen($"SFXDeathScream{soundNumber}", CameraView.IsInsideCamera(transform.position, Cam));
    }
    private void DeadWallDeath()
    {
        isDead = true;
        if (healthBar != null)
        { healthBar.Hide(); }
        ObjectPoolManager.ReturnObjectToPool(gameObject);
        flash.ResetMaterials();
    }
    public void SetTutorialEnemy()
    {
        IsTutorialEnemy = true;
    }

    //---------------------GIZMOS-------------------------
    void OnDrawGizmosSelected()
    {
        if (targetWagon.Middle != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetWagon.Middle);
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