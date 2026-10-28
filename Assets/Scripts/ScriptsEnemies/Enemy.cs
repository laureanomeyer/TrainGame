using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region EnemyStates

public enum EnemyHealthState
{
    Normal,
    Armored,
    Grace,
    Dead

}
public enum EnemyMovementState
{
    Moving,
    Losing,
    Waiting,
    Ended,
    RunEnd
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
    SkinnedMeshRenderer enemyRend;
    SkinnedMeshRenderer horseRend;
    [Header("UI")]
    [SerializeField] private EnemyUIHpBar healthBar;
    [SerializeField] private GameObject buffParticles;

    [SerializeField] private EnemyMovementSO deadMovement, endMovement;
    public EnemyWeapon Weapon ;
    BoxCollider boxCollider;
    public string TargetWagonName;
    public string dataName;
    public string movementName;
    public string MovementStateName; 
    public string AttackStateName;
    public string HealthStateName;

    private EnemyData data;
    private IWagon targetWagon;
    private float currentHealth;
    private Coroutine attackRoutine;
    private TrainRanges trainRanges;
    private (float, float) limits;
    public (float, float) Limits => limits;
    public EnemyUIHpBar HealthBar => healthBar;
    private EnemyAnimationSO ActiveAnimation => data.animation;

    #region Data
    public EnemyMovementSO Movement {get ; private set;} 
    public EnemyAttackSO Attack => data.attack;
    public EnemyBrainSO Brain => data.brain;
    public Rigidbody rb;
    public float Speed => data.movement.speed;
    public float MaxHealth => data.health;
    public float Damage {get ; private set;}
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
    
    #endregion
    public Transform VisualAnchor => visualAnchor;

    void Awake()
    {
        if (visualAnchor == null)
        {
            visualAnchor = transform.Find("VisualAnchor");
        }

        if (visualAnchor == null)
        {
            visualAnchor = new GameObject("VisualAnchor").transform;
            visualAnchor.SetParent(transform, false);
            visualAnchor.localPosition = Vector3.zero;
            visualAnchor.localRotation = Quaternion.identity;
            visualAnchor.localScale = Vector3.one;
        }

        Weapon = GetComponentInChildren<EnemyWeapon>();
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

    }
    void OnEnable()
    {
        EventBus.Subscribe<OnSetTutorialEnemyTarget>(SetSingleTargetByEvent);
        EventBus.Subscribe<OnWagonDestroyedEvent>(RetargetWagon);
    }
    void OnDisable()
    {
        EventBus.Unsubscribe<OnSetTutorialEnemyTarget>(SetSingleTargetByEvent);
        EventBus.Unsubscribe<OnWagonDestroyedEvent>(RetargetWagon);
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
        Damage = data.damage;

        Movement.Begin(this);
        Skill.Begin(this);
        dataName = data.name;

        SetAnimators();
        SetNewCollider();

        if (healthBar)
        {
            healthBar.SetHealth(currentHealth, MaxHealth);
            healthBar.ShowArmor(HealthState == EnemyHealthState.Armored);
            healthBar.ShowBuff(AttackState == EnemyAttackState.Buffed);
        }

        PlayIdleAnimation();

        trainRanges = new();
        limits = trainRanges.SetRanges(Range, Vector3.zero);

        if (!IsTutorialEnemy)
        { 
            targetWagon = Brain.GetPreference(data.targetPreference);
        }
    }

    public void SetNewCollider()
    {
        if (activeVisualRoot == null || boxCollider == null)
            return;

        BoxCollider visualCollider = activeVisualRoot.GetComponent<BoxCollider>();

        if (visualCollider == null)
        {
            Debug.LogWarning($"No BoxCollider found on {activeVisualRoot.name}");
            return;
        }

        Transform visualTransform = visualCollider.transform;
        Transform targetTransform = boxCollider.transform;

        // Convert the visual collider's center to the target's local space.
        Vector3 worldCenter = visualTransform.TransformPoint(visualCollider.center);
        Vector3 localCenter = targetTransform.InverseTransformPoint(worldCenter);

        // Get the eight corners of the visual collider in local space.
        Vector3 halfSize = visualCollider.size * 0.5f;

        Vector3 min = visualCollider.center - halfSize;
        Vector3 max = visualCollider.center + halfSize;

        Vector3[] corners =
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z)
        };

        Bounds localBounds = new Bounds(localCenter, Vector3.zero);

        foreach (Vector3 corner in corners)
        {
            Vector3 worldCorner = visualTransform.TransformPoint(corner);
            Vector3 targetLocalCorner = targetTransform.InverseTransformPoint(worldCorner);

            localBounds.Encapsulate(targetLocalCorner);
        }

        boxCollider.center = localBounds.center;
        boxCollider.size = localBounds.size;

        visualCollider.enabled = false;

        Debug.Log(
            $"Collider updated: {boxCollider.name}\n" +
            $"Center: {boxCollider.center}\n" +
            $"Size: {boxCollider.size}"
        );
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
        HealthStateName = healthState.ToString();
    }

    public void ChangeState(EnemyMovementState state)
    {
        if (movementState == state) return;
        if (movementState == EnemyMovementState.RunEnd)return;
        movementState = state;
        if (state == EnemyMovementState.Waiting)
        {
            timeAtTarget = Time.time;
        }
        if (state == EnemyMovementState.Ended)
        {
            Movement = deadMovement;
        }
        if (state == EnemyMovementState.RunEnd)
        {
            Movement = endMovement;
        }
        Movement.Begin(this);
        movementName = Movement.name;
        MovementStateName = movementState.ToString(); 
    }

    public void ChangeState(EnemyAttackState state)
    {
        attackState = state;
        if (attackState == EnemyAttackState.Buffed)
        {
            buffParticles.SetActive(true);
        }
        else
        {
            buffParticles.SetActive(false);
        }
        AttackStateName = attackState.ToString();
    }

    public void SetAtTarget(bool atTarget) // revisar
    {
        if (movementState == EnemyMovementState.Losing) return;
        ChangeState(atTarget ? EnemyMovementState.Waiting : EnemyMovementState.Moving);
    }

    public void AddDamage(float damageIncrease)
    {
        Damage =+ damageIncrease;
    }
    void Update()
    {
        if (healthState == EnemyHealthState.Grace && movementState == EnemyMovementState.Ended && CameraView.IsOutsideCamera(transform.position, Cam))
        {
            Despawn();
        }
        else if (healthState == EnemyHealthState.Dead || healthState == EnemyHealthState.Grace)
        {
            return;
        }
        else
        {
            attackCooldownTimer -= Time.deltaTime;
            skillCooldownTimer -= Time.deltaTime;
            Attack?.Attack(this);
            Attack?.Skill(this);
        }

    }
    void FixedUpdate()
    {
        if (HealthState == EnemyHealthState.Dead) return;
        if (movementState == EnemyMovementState.Ended && healthState == EnemyHealthState.Grace)
        {
            animations.ActivateCowboyOffset();
        }
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

    EnemyAnimationsContainer animations;
    [SerializeField] private VisualsFactory visualsFactory;
    private void SetAnimators()
    {
        activeVisualRoot = visualsFactory.GetVisual(
            data.visualType,
            visualAnchor
        );

        if (activeVisualRoot == null)
            return;

        animations = activeVisualRoot
            .GetComponentInChildren<EnemyAnimationsContainer>(true);
    }
    public void PlayIdleAnimation()
    {
        PlayCowboyAnimation(GetAnimationName(
            IsOnPositiveZSide ? "Cowboy_1|L_Idle" : "Cowboy_1|R_Idle",
            IsOnPositiveZSide ? ActiveAnimation?.positiveZIdle : ActiveAnimation?.negativeZIdle));

        PlayHorseAnimation(ActiveAnimation?.horseIdle ?? "Horse|Idle");
    }

    public void PlayAttackAnimation()
    {
        if (attackRoutine != null) return;
        PlayCowboyAnimation(GetAnimationName(
            IsOnPositiveZSide ? "Cowboy_1|L_Aim" : "Cowboy_1|R_Aim 0",
            IsOnPositiveZSide ? ActiveAnimation?.positiveZAttack : ActiveAnimation?.negativeZAttack));

        attackRoutine = StartCoroutine(ReturnToIdleAfterAttack());
    }

    private string GetAnimationName(string defaultName, string configuredName)
    {
        return string.IsNullOrEmpty(configuredName) ? defaultName : configuredName;
    }

    private IEnumerator ReturnToIdleAfterAttack()
    {
        yield return new WaitForSeconds(ActiveAnimation?.attackDuration ?? 0.8f);

        if (HealthState != EnemyHealthState.Grace) PlayIdleAnimation();
        attackRoutine = null;
    }

    private void PlayCowboyAnimation(string stateName)
    {
        if (animations == null)
            return;

        string layerName = IsOnPositiveZSide
            ? "Left Layer"
            : "Right Layer";

        animations.PlayCowboyAnimation(stateName, layerName);
    }

    private void PlayHorseAnimation(string stateName)
    {
        if (animations == null)
            return;

        animations.PlayHorseAnimation(stateName);
    }
    private IEnumerator ReturnAfterDeathAnimation()
    {
        yield return new WaitForSeconds(0.7f); //  
        ChangeState(EnemyMovementState.Ended);
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
    private void Die() //publica eventos y animaciones
    {
        if (HealthState == EnemyHealthState.Dead) return;

        ChangeState(EnemyHealthState.Grace);
        PlayDeathSound();
        PlayCowboyAnimation(GetAnimationName(
            IsOnPositiveZSide ? "Cowboy_1|L_Death" : "Cowboy_1|R_Death",
            IsOnPositiveZSide
                ? ActiveAnimation?.positiveZDeath
                : ActiveAnimation?.negativeZDeath));
        
        if (healthBar != null)
        { healthBar.Hide(); }
        Skill.Stop(this);
        EventBus.Publish(new OnEnemyDeathEvent(transform.position, data.drop, data.dropAmount));
        EventBus.Publish(new OnEnemyKilledEvent());
        StartCoroutine(ReturnAfterDeathAnimation()); //termina animacion y empieza a perder
    }
    void PlayDeathSound()
    {
        int soundNumber = Random.Range(1, 1001) == 1000 ? 4 : Random.Range(1, 4);
        AudioManager.Instance.PlayOnScreen($"SFXDeathScream{soundNumber}", CameraView.IsInsideCamera(transform.position, Cam));
    }
    public void Despawn()
    {

        ChangeState(EnemyHealthState.Dead);
        StopAllCoroutines();
        attackRoutine = null;
        PublishBecameInactive();

        if (healthBar != null)
            healthBar.Hide();
        
        if (activeVisualRoot != null)
        {
            visualsFactory.ReturnVisual(activeVisualRoot);
            activeVisualRoot = null;
            animations = null;
        }

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