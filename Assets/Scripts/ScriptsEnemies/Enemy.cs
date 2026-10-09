using System.Collections.Generic;
using UnityEngine;

#region EnemyStates

public enum EnemyState
{
    EnemyHealthState,
    EnemyMovementState,
    EnemyAttackState

}

public enum EnemyHealthState
{
    Normal,
    Armored,
    Dead
}
public enum EnemyMovementState
{
    Moving,
    Losing,
    Waiting
}

public enum EnemyAttackState
{
    Normal,
    Buffed,
}

#endregion

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
    [SerializeField] private EnemyMovementSO endMovement;
    public EnemyWeapon Weapon;
    BoxCollider boxCollider;

    public string TargetWagonName;
    public string dataName;
    private EnemyData data;
    private IWagon targetWagon;
    private float currentHealth;
    private DamageFlash flash;
    private int activeCowboyLayer;
    private Coroutine attackRoutine;
    private TrainRanges trainRanges;
    private (float, float) limits;
    public (float, float) Limits => limits;

    public EnemyUIHpBar HealthBar => healthBar;

    #region Data
    public EnemyMovementSO Movement {get ; private set;} 
    public EnemyAttackSO Attack => data.attack;
    public EnemyBrainSO Brain => data.brain;
    public Rigidbody rb;
    public float Speed => data.movement.speed;
    public float MaxHealth => data.health;
    public float Damage => data.damage;
    public float Cooldown => data.attackCooldown;
    public DropType Drop => data.drop;
    public IWagon TargetWagon => targetWagon;
    public EnemyData Data => data;

    public float Range => (float)data.rangeType;
    private EnemyHealthState healthState;
    private EnemyMovementState movementState;
    private EnemyAttackState attackState;

    #endregion

    #region Booleans
    private bool inactiveEventPublished;

    float attackCooldownTimer;
    float skillCooldownTimer;
    public bool CanAttack => attackCooldownTimer <= 0f;
    public bool CanSkill => skillCooldownTimer <= 0f;

    public EnemySkillSO Skill => data.skill;
    private float timeAtTarget;
    public EnemyHealthState HealthState => healthState;
    public EnemyMovementState MovementState => movementState;
    public EnemyAttackState AttackState => attackState;
    public float TimeAtTarget =>
        movementState == EnemyMovementState.Waiting
            ? Time.time - timeAtTarget
            : 0f;
    public bool IsOnPositiveZSide =>
        rb != null && rb.position.z >= 0f;
    public bool IsOnNegativeZSide =>
        rb != null && rb.position.z < 0f;
    public Camera Cam => Camera.main;
    public bool IsTutorialEnemy { get; private set; }
    int rightLayerIndex;
    int leftLayerIndex;
    #endregion

    void Awake()
    {
        Weapon = GetComponentInChildren<EnemyWeapon>();
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

        rightLayerIndex = cowboyAnimator.GetLayerIndex("Right Layer");
        leftLayerIndex = cowboyAnimator.GetLayerIndex("Left Layer");

        EventBus.Subscribe<OnSetTutorialEnemyTarget>(SetSingleTargetByEvent);
        EventBus.Subscribe<OnWagonDestroyedEvent>(RetargetWagon);
        EventBus.Subscribe<OnRunEndedEvent>(ChangeMovement);
    }
    void OnDestroy()
    {
        EventBus.Unsubscribe<OnSetTutorialEnemyTarget>(SetSingleTargetByEvent);
        EventBus.Unsubscribe<OnWagonDestroyedEvent>(RetargetWagon);
        EventBus.Unsubscribe<OnRunEndedEvent>(ChangeMovement);

    }

    public void Initialize(EnemyData data)
    {
        StopAllCoroutines();
        attackRoutine = null;

        ChangeState(EnemyHealthState.Normal);
        ChangeState(EnemyMovementState.Moving);
        ChangeState(EnemyAttackState.Normal);
        inactiveEventPublished = false;
        timeAtTarget = 0f;
        IsTutorialEnemy = false;
        targetWagon = null;
        this.data = data;

        Movement = data.movement;
        currentHealth = MaxHealth;
        skillCooldownTimer = Skill.Cooldown;
        attackCooldownTimer = data.attackCooldown;

        Movement.Begin(this);

        dataName = data.name;

        if (enemyRend) enemyRend.sharedMesh = data.enemyMesh.sharedMesh;
        if (horseRend) horseRend.sharedMesh = data.horseMesh.sharedMesh;
        if (healthBar) healthBar.SetHealth(currentHealth, MaxHealth);

        healthBar.ShowArmor(HealthState == EnemyHealthState.Armored);

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

    public void ChangeState(EnemyHealthState state)
    {
        healthState = state;
    }

    public void ChangeState(EnemyMovementState state)
    {
        if (movementState == state) return;

        movementState = state;
        if (state == EnemyMovementState.Waiting)
            timeAtTarget = Time.time;
    }

    public void ChangeState(EnemyAttackState state)
    {
        attackState = state;
    }

    public void SetAtTarget(bool atTarget)
    {
        if (movementState == EnemyMovementState.Losing) return;

        ChangeState(atTarget ? EnemyMovementState.Waiting : EnemyMovementState.Moving);
    }

    public void BeginLosing()
    {
        ChangeState(EnemyMovementState.Losing);
    }

    void Update()
    {
        if (healthState == EnemyHealthState.Dead) return;


        attackCooldownTimer -= Time.deltaTime;
        skillCooldownTimer -= Time.deltaTime;
        Attack?.Attack(this);
        Attack?.Skill(this);

    }

    void FixedUpdate()
    {
        if (HealthState == EnemyHealthState.Dead) return;

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

    public void ChangeMovement(OnRunEndedEvent ev)
    {
        Movement = endMovement;
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

        if (HealthState != EnemyHealthState.Dead) PlayIdleAnimation();
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
        switch (HealthState)
        {
            case EnemyHealthState.Normal:
                ApplyDamage(damage);
                return currentHealth <= 0;
            case EnemyHealthState.Armored:
                ApplyDamage(damage -1 );
                healthBar.ShowArmor(false);
                ChangeState(EnemyHealthState.Normal);
                return true;
            case EnemyHealthState.Dead:
                return false;
            default:
                return false;
        }
    }
    private void ApplyDamage(float damage)
    {
        currentHealth -= damage;
        EventBus.Publish(new OnEnemyHitEvent(transform.position));
        DamagePopupManager.Instance?.ShowDamage(damage, transform.position);
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, MaxHealth);
        }
        if (currentHealth <= 0)
            Die();
    }
    private void Die()
    {
        if (HealthState == EnemyHealthState.Dead) return;

        ChangeState(EnemyHealthState.Dead);
        PublishBecameInactive();
        PlayDeathSound();
        PlayCowboyAnimation(GetAnimationName(
            IsOnPositiveZSide ? "Cowboy_1|L_Death" : "Cowboy_1|R_Death",
            IsOnPositiveZSide
                ? data.animation?.positiveZDeath
                : data.animation?.negativeZDeath));

        if (healthBar != null)
        { healthBar.Hide(); }
        flash.ResetMaterials();
        SpawnDrop();
        EventBus.Publish(new OnEnemyDeathEvent(transform.position, data.drop));
        EventBus.Publish(new OnEnemyKilledEvent());
        StartCoroutine(ReturnAfterDeathAnimation());
    }

    void SpawnDrop()
    {
        if (Drop == DropType.Coal)
        {
            EventBus.Publish(new OnCoalEarnedEvent(data.dropAmount));
        }
        else if (Drop == DropType.Gold)
        {
            EventBus.Publish(new OnGoldEarnedEvent(data.dropAmount));
        }
    }

    void PlayDeathSound()
    {
        int soundNumber = Random.Range(1, 1001) == 1000 ? 4 : Random.Range(1, 4);
        AudioManager.Instance.PlayOnScreen($"SFXDeathScream{soundNumber}", CameraView.IsInsideCamera(transform.position, Cam));
    }
    public void Despawn()
    {
        if (HealthState == EnemyHealthState.Dead || !gameObject.activeSelf) return;

        ChangeState(EnemyHealthState.Dead);
        StopAllCoroutines();
        attackRoutine = null;
        PublishBecameInactive();

        if (healthBar != null)
            healthBar.Hide();
        flash?.ResetMaterials();

        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    private void PublishBecameInactive()
    {
        if (inactiveEventPublished) return;

        inactiveEventPublished = true;
        EventBus.Publish(new OnEnemyBecameInactiveEvent(this));
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
            Despawn();
        }
    }
}