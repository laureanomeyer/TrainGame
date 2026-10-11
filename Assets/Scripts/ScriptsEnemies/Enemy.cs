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
    [Header("Visual root")]
    [SerializeField] private Transform visualAnchor;

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
    private GameObject activeVisualRoot;

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

    public Transform VisualAnchor => visualAnchor;

    void Awake()
    {

        Weapon = GetComponentInChildren<EnemyWeapon>();
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

        if (cowboyAnimator != null)
        {
            rightLayerIndex = cowboyAnimator.GetLayerIndex("Right Layer");
            leftLayerIndex = cowboyAnimator.GetLayerIndex("Left Layer");
        }

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

        activeVisualInstance = Instantiate(DefaultVisualPrefab, visualAnchor);
        activeVisualInstance.SetActive(false);

        InitializeVisuals();

        if (healthBar) healthBar.SetHealth(currentHealth, MaxHealth);
        healthBar.ShowArmor(HealthState == EnemyHealthState.Armored);

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

    public bool TakeDamage(float damage)
    {
        switch (HealthState)
        {
            case EnemyHealthState.Normal:
                ApplyDamage(damage);
                return currentHealth <= 0;
            case EnemyHealthState.Armored:
                ApplyDamage(damage - 1 );
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
        if (healthBar != null)
        { healthBar.Hide(); }
        flash.ResetMaterials();
        EventBus.Publish(new OnEnemyDeathEvent(transform.position, data.drop, data.dropAmount));
        EventBus.Publish(new OnEnemyKilledEvent());
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

    #region Animations & Visuals
    [SerializeField] GameObject DefaultVisualPrefab;
    List<GameObject> activeVisuals = new List<GameObject>();
    GameObject activeVisualInstance = null;
    public (bool, int) CheckVisualsPool(GameObject prefab)
    {
        foreach (var visual in activeVisuals)
        {
            if (visual == prefab)
                return (false, -1);
        }
        return (true, activeVisuals.IndexOf(prefab));
    }
    public void InitializeVisuals()
    {
        if (CheckVisualsPool(data.visual.visualPrefab).Item1 && CheckVisualsPool(data.visual.visualPrefab).Item2 == -1)
        {
            activeVisualInstance = Instantiate(data.visual.visualPrefab, visualAnchor);
            activeVisuals.Add(activeVisualInstance);
        }
        else
        {
            activeVisuals[CheckVisualsPool(data.visual.visualPrefab).Item2].SetActive(true);
        }
        if (!activeVisualInstance.activeSelf)
        {
            activeVisualInstance.SetActive(true);
        }
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