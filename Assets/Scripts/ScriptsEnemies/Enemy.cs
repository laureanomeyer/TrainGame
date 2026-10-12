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
    [Header("UI")]
    [SerializeField] private EnemyUIHpBar healthBar;
    [Header("Animators")]
    [SerializeField] Animator cowboyAnimator;
    [SerializeField] Animator horseAnimator;
    private AnimatorBrain leftCowboyBrain = new AnimatorBrain();
    private AnimatorBrain rightCowboyBrain = new AnimatorBrain();
    private AnimatorBrain horseBrain = new AnimatorBrain();
    private EnemyAnimationSO animationData;
    private AnimatorBrain pendingAttackBrain;
    private IWagon pendingAttackTarget;
    private float pendingAttackDamage;
    private float pendingAttackFiringTime;
    private bool hasPendingAttack;
    private AnimatorBrain deathBrain;
    private bool deathAnimationRunning;
    private bool returnedToPool;
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
    int rightLayerIndex = -1;
    int leftLayerIndex = -1;
    #endregion

    public Transform VisualAnchor => visualAnchor;

    void Awake()
    {

        Weapon = GetComponentInChildren<EnemyWeapon>();
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

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
        CancelPendingAttack();
        deathBrain = null;
        deathAnimationRunning = false;
        returnedToPool = false;

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

        ApplyVisual(data.visual);

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
        RefreshCowboyLayerWeights();
        if (healthState == EnemyHealthState.Dead)
        {
            UpdateDeathAnimation();
            return;
        }

        attackCooldownTimer -= Time.deltaTime;
        skillCooldownTimer -= Time.deltaTime;
        AdvancePendingAttack();
        TickAnimationBrains();
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
        CancelPendingAttack();
        ChangeState(EnemyHealthState.Dead);
        PublishBecameInactive();
        PlayDeathSound();
        if (healthBar != null)
        { healthBar.Hide(); }
        flash?.ResetMaterials();
        EventBus.Publish(new OnEnemyDeathEvent(transform.position, data.drop, data.dropAmount));
        EventBus.Publish(new OnEnemyKilledEvent());

        if (!StartDeathAnimation())
        {
            ReturnToPoolOnce();
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
        CancelPendingAttack();
        deathAnimationRunning = false;
        deathBrain = null;
        PublishBecameInactive();

        if (healthBar != null)
            healthBar.Hide();
        flash?.ResetMaterials();

        ReturnToPoolOnce();
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
    Dictionary<GameObject, GameObject> visualPool = new Dictionary<GameObject, GameObject>();
    GameObject activeVisualInstance = null;

    public void InitializeVisuals()
    {
        if (data == null || data.visual == null)
        {
            Debug.LogError($"Enemy visual data is missing for {name}.", this);
            return;
        }

        SelectVisual(data.visual.visualPrefab);
    }

    private void ApplyVisual(EnemyVisualSO visual)
    {
        ResetAnimationBindings();

        if (visual == null)
        {
            Debug.LogError($"Enemy visual definition is missing for {name}.", this);
            return;
        }

        if (!SelectVisual(visual.visualPrefab))
        {
            return;
        }

        flash = GetComponent<DamageFlash>();
        if (flash != null)
        {
            flash.StopCoroutine();
            flash.SetRenderers(activeVisualInstance.GetComponentsInChildren<Renderer>(true));
        }

        InitializeAnimations(visual);
    }

    private void ResetAnimationBindings()
    {
        CancelPendingAttack();
        deathBrain = null;
        deathAnimationRunning = false;
        animationData = null;
        cowboyAnimator = null;
        horseAnimator = null;
        leftLayerIndex = -1;
        rightLayerIndex = -1;
        leftCowboyBrain.SetIdle(default);
        rightCowboyBrain.SetIdle(default);
        horseBrain.SetIdle(default);
        leftCowboyBrain.Init(null);
        rightCowboyBrain.Init(null);
        horseBrain.Init(null);
    }

    private bool SelectVisual(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError($"Enemy visual prefab is missing for {name}.", this);
            return false;
        }

        if (visualAnchor == null)
        {
            visualAnchor = transform;
        }

        if (activeVisualInstance != null)
        {
            activeVisualInstance.SetActive(false);
        }

        if (!visualPool.TryGetValue(prefab, out activeVisualInstance))
        {
            activeVisualInstance = Instantiate(prefab, visualAnchor);
            visualPool.Add(prefab, activeVisualInstance);
        }

        activeVisualInstance.SetActive(true);
        return true;
    }

    public void InitializeAnimations()
    {
        if (data == null)
        {
            Debug.LogError($"Enemy animation data is missing for {name}.", this);
            return;
        }

        InitializeAnimations(data.visual);
    }

    private void InitializeAnimations(EnemyVisualSO visual)
    {
        if (activeVisualInstance == null)
        {
            Debug.LogError($"Enemy visual instance is missing for {name}.", this);
            return;
        }

        EnemyVisualAnimatorBindings bindings = activeVisualInstance.GetComponent<EnemyVisualAnimatorBindings>();
        if (bindings == null)
        {
            Debug.LogError($"Enemy visual '{activeVisualInstance.name}' is missing EnemyVisualAnimatorBindings.", this);
            return;
        }

        cowboyAnimator = bindings.CowboyAnimator;
        horseAnimator = bindings.MountAnimator;
        if (cowboyAnimator == null || horseAnimator == null)
        {
            Debug.LogError($"Enemy visual '{activeVisualInstance.name}' has a missing cowboy or mount Animator binding.", this);
            return;
        }

        ApplyAnimatorOverrides(
            cowboyAnimator,
            visual.cowboyAnimatorOverride,
            visual.cowboyClipOverrides,
            "cowboy");
        ApplyAnimatorOverrides(
            horseAnimator,
            visual.mountAnimatorOverride,
            visual.mountClipOverrides,
            "mount");

        leftLayerIndex = cowboyAnimator.GetLayerIndex("Left Layer");
        rightLayerIndex = cowboyAnimator.GetLayerIndex("Right Layer");
        if (leftLayerIndex < 0 || rightLayerIndex < 0)
        {
            Debug.LogError($"Enemy visual '{activeVisualInstance.name}' cowboy Animator is missing the Left Layer or Right Layer.", this);
            return;
        }

        if (visual.animations == null)
        {
            Debug.LogError($"Enemy visual '{activeVisualInstance.name}' is missing animation data.", this);
            return;
        }

        animationData = visual.animations;
        if (!HasRequiredState(cowboyAnimator, leftLayerIndex, visual.animations.positiveZIdle, "positive-Z idle", visual) ||
            !HasRequiredState(cowboyAnimator, leftLayerIndex, visual.animations.positiveZAttack, "positive-Z attack", visual) ||
            !HasRequiredState(cowboyAnimator, leftLayerIndex, visual.animations.positiveZDeath, "positive-Z death", visual) ||
            !HasRequiredState(cowboyAnimator, rightLayerIndex, visual.animations.negativeZIdle, "negative-Z idle", visual) ||
            !HasRequiredState(cowboyAnimator, rightLayerIndex, visual.animations.negativeZAttack, "negative-Z attack", visual) ||
            !HasRequiredState(cowboyAnimator, rightLayerIndex, visual.animations.negativeZDeath, "negative-Z death", visual) ||
            !HasRequiredState(horseAnimator, 0, visual.animations.horseIdle, "horse idle", visual))
        {
            return;
        }

        leftCowboyBrain.SetIdle(visual.animations.positiveZIdle);
        rightCowboyBrain.SetIdle(visual.animations.negativeZIdle);
        horseBrain.SetIdle(visual.animations.horseIdle);
        leftCowboyBrain.Init(cowboyAnimator, leftLayerIndex);
        rightCowboyBrain.Init(cowboyAnimator, rightLayerIndex);
        horseBrain.Init(horseAnimator);
        RefreshCowboyLayerWeights();
    }

    private bool HasRequiredState(Animator animator, int layerIndex, AnimationState state, string description, EnemyVisualSO visual)
    {
        if (animator != null && layerIndex >= 0 &&
            state.FullPathHash != 0 && animator.HasState(layerIndex, state.FullPathHash))
        {
            return true;
        }

        Debug.LogError($"Enemy animation '{description}' state path is missing in '{visual.name}'.", this);
        return false;
    }

    private bool StartDeathAnimation()
    {
        if (animationData == null || cowboyAnimator == null)
        {
            Debug.LogError($"Enemy death animation data or cowboy Animator is missing for {name}.", this);
            return false;
        }

        bool positiveZSide = IsOnPositiveZSide;
        AnimationState death = positiveZSide
            ? animationData.positiveZDeath
            : animationData.negativeZDeath;
        int layerIndex = positiveZSide ? leftLayerIndex : rightLayerIndex;
        if (death.FullPathHash == 0 || layerIndex < 0 ||
            !cowboyAnimator.HasState(layerIndex, death.FullPathHash))
        {
            string side = positiveZSide ? "positive-Z" : "negative-Z";
            Debug.LogError($"Enemy animation '{side} death' state path is missing for {name}.", this);
            return false;
        }

        deathBrain = positiveZSide ? leftCowboyBrain : rightCowboyBrain;
        if (!deathBrain.Play(death, lockAnimation: true, overrideLock: true))
        {
            Debug.LogError($"Enemy death animation could not start for {name}.", this);
            deathBrain = null;
            return false;
        }

        deathAnimationRunning = true;
        return true;
    }

    private void UpdateDeathAnimation()
    {
        if (!deathAnimationRunning || deathBrain == null)
        {
            ReturnToPoolOnce();
            return;
        }

        if (deathBrain.Finished())
        {
            deathAnimationRunning = false;
            ReturnToPoolOnce();
            return;
        }

        TickAnimationBrains();
    }

    private void ReturnToPoolOnce()
    {
        if (returnedToPool || !gameObject.activeSelf)
        {
            return;
        }

        returnedToPool = true;
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    public bool TryQueueRangedAttack(IWagon target, float damage)
    {
        if (!IsWagonAvailable(target) || hasPendingAttack)
        {
            return false;
        }

        if (Weapon == null)
        {
            Debug.LogError($"Enemy weapon is missing for ranged attack on {name}.", this);
            return false;
        }

        if (!Weapon.CanShoot)
        {
            return false;
        }

        if (animationData == null)
        {
            Debug.LogError($"Enemy animation data is missing for ranged attack on {name}.", this);
            return false;
        }

        AnimatorBrain brain = IsOnPositiveZSide ? leftCowboyBrain : rightCowboyBrain;
        AnimationState attack = IsOnPositiveZSide
            ? animationData.positiveZAttack
            : animationData.negativeZAttack;
        int layerIndex = IsOnPositiveZSide ? leftLayerIndex : rightLayerIndex;
        if (cowboyAnimator == null || layerIndex < 0 ||
            !cowboyAnimator.HasState(layerIndex, attack.FullPathHash))
        {
            Debug.LogError($"Enemy ranged attack animation state is missing for {name}.", this);
            return false;
        }

        if (!brain.Play(attack, lockAnimation: true))
        {
            return false;
        }

        pendingAttackBrain = brain;
        pendingAttackTarget = target;
        pendingAttackDamage = damage;
        pendingAttackFiringTime = Mathf.Clamp01(animationData.firingNormalizedTime);
        hasPendingAttack = true;
        return true;
    }

    public void AdvancePendingAttack()
    {
        if (!hasPendingAttack)
        {
            return;
        }

        if (Weapon == null || !Weapon.CanShoot ||
            !IsWagonAvailable(pendingAttackTarget) ||
            pendingAttackTarget != targetWagon || !IsWagonAvailable(targetWagon) ||
            pendingAttackBrain == null)
        {
            AbortPendingAttack();
            return;
        }

        float normalizedTime = pendingAttackBrain.CurrentNormalizedTime;
        if (normalizedTime < pendingAttackFiringTime)
        {
            return;
        }

        IWagon target = pendingAttackTarget;
        float damage = pendingAttackDamage;
        CancelPendingAttack();
        Weapon.Execute(target, damage);
    }

    private static bool IsWagonAvailable(IWagon wagon)
    {
        if (wagon == null || (wagon is Object unityObject && unityObject == null))
        {
            return false;
        }

        return wagon.Head != null && wagon.Tail != null;
    }

    public void CancelPendingAttack()
    {
        hasPendingAttack = false;
        pendingAttackBrain = null;
        pendingAttackTarget = null;
        pendingAttackDamage = 0f;
        pendingAttackFiringTime = 0f;
    }

    private void AbortPendingAttack()
    {
        AnimatorBrain brain = pendingAttackBrain;
        CancelPendingAttack();
        brain?.ReturnToIdle();
    }

    private void TickAnimationBrains()
    {
        leftCowboyBrain.Tick();
        rightCowboyBrain.Tick();
        horseBrain.Tick();
    }

    private void ApplyAnimatorOverrides(
        Animator animator,
        AnimatorOverrideController sourceOverride,
        AnimationClipOverride[] clipOverrides,
        string animatorDescription)
    {
        if (sourceOverride != null)
        {
            animator.runtimeAnimatorController = sourceOverride;
        }

        if (clipOverrides == null || clipOverrides.Length == 0)
        {
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError($"Enemy visual '{activeVisualInstance.name}' has no controller for its {animatorDescription} Animator.", this);
            }

            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError($"Enemy visual '{activeVisualInstance.name}' has no controller for its {animatorDescription} Animator.", this);
            return;
        }

        AnimatorOverrideController instanceOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);
        List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        instanceOverride.GetOverrides(overrides);

        foreach (AnimationClipOverride clipOverride in clipOverrides)
        {
            if (clipOverride == null || clipOverride.baseClip == null || clipOverride.replacementClip == null)
            {
                Debug.LogError($"Enemy visual '{activeVisualInstance.name}' has an incomplete {animatorDescription} clip override.", this);
                continue;
            }

            int index = overrides.FindIndex(entry => entry.Key == clipOverride.baseClip);
            if (index < 0)
            {
                Debug.LogError($"Enemy visual '{activeVisualInstance.name}' cannot find base clip '{clipOverride.baseClip.name}' in its {animatorDescription} controller.", this);
                continue;
            }

            overrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(clipOverride.baseClip, clipOverride.replacementClip);
        }

        instanceOverride.ApplyOverrides(overrides);
        animator.runtimeAnimatorController = instanceOverride;
    }

    private void RefreshCowboyLayerWeights()
    {
        if (cowboyAnimator == null || rb == null ||
            leftLayerIndex < 0 || rightLayerIndex < 0 ||
            leftLayerIndex >= cowboyAnimator.layerCount || rightLayerIndex >= cowboyAnimator.layerCount)
        {
            return;
        }

        bool positiveZSide = IsOnPositiveZSide;
        cowboyAnimator.SetLayerWeight(leftLayerIndex, positiveZSide ? 1f : 0f);
        cowboyAnimator.SetLayerWeight(rightLayerIndex, positiveZSide ? 0f : 1f);
    }

    #endregion
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