using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerBrain brain;
    [SerializeField] private Rigidbody rb;

    [Header("Weapon overrides")]
    [Tooltip("Un Animator Override Controller por tipo de arma, en el mismo orden que el enum (índice = valor del enum). " +
             "Todos deben apuntar al mismo controller base que tiene el Animator.")]
    [SerializeField] private AnimatorOverrideController[] weaponOverrides;

    private float maxSpeed;

    private int SpeedHash = Animator.StringToHash("Speed");
    private int IsRepairingHash = Animator.StringToHash("IsRepairing");

    private AnimatorOverrideController runtimeOverride;
    private List<KeyValuePair<AnimationClip, AnimationClip>> runtimePairs = new();
    private List<KeyValuePair<AnimationClip, AnimationClip>> sourcePairs = new();
    private Dictionary<AnimationClip, AnimationClip> lookup = new();

    private bool lastRepairing;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (brain == null) brain = GetComponent<PlayerBrain>();
        if (rb == null) rb = GetComponent<Rigidbody>();

        maxSpeed = brain.Speed;

        runtimeOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = runtimeOverride;

        EventBus.Subscribe<OnWeaponChangedEvent>(HandleWeaponChanged);
    }

    private void Start()
    {
        RefreshWeapon();
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnWeaponChangedEvent>(HandleWeaponChanged);
    }

    private void Update()
    {
        if (animator == null || brain == null) return;

        UpdateRepairing();
        UpdateSpeed();
    }

    private void HandleWeaponChanged(OnWeaponChangedEvent ev)
    {
        RefreshWeapon();
    }

    /// <summary>
    /// Lee el tipo de arma actual del attack controller y aplica el override correspondiente.
    /// </summary>
    public void RefreshWeapon()
    {
        var attack = brain != null ? brain.PlayerAttackController : null;
        if (attack == null) return;

        ApplyWeaponOverride((int)attack.CurrentWeaponType);
    }

    private void ApplyWeaponOverride(int weaponType)
    {
        if (weaponOverrides == null || weaponType < 0 || weaponType >= weaponOverrides.Length) return;

        var source = weaponOverrides[weaponType];
        if (source == null) return;

        // Clips que reemplaza esta arma (los slots en None no se tocan).
        lookup.Clear();
        source.GetOverrides(sourcePairs);
        foreach (var pair in sourcePairs)
        {
            if (pair.Value != null) lookup[pair.Key] = pair.Value;
        }

        // Lo que el arma no reemplaza vuelve al clip original del base.
        runtimeOverride.GetOverrides(runtimePairs);
        for (int i = 0; i < runtimePairs.Count; i++)
        {
            AnimationClip original = runtimePairs[i].Key;
            AnimationClip replacement = lookup.TryGetValue(original, out var clip) ? clip : null;
            runtimePairs[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, replacement);
        }

        runtimeOverride.ApplyOverrides(runtimePairs);
    }

    private void UpdateRepairing()
    {
        bool repairing = brain.IsRepairing;
        if (repairing == lastRepairing) return;

        lastRepairing = repairing;
        animator.SetBool(IsRepairingHash, repairing);
    }

    private void UpdateSpeed()
    {
        float normalized = 0f;

        if (!brain.IsRepairing && rb != null && maxSpeed > 0f)
        {
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            normalized = Mathf.Clamp01(v.magnitude / maxSpeed);
        }

        animator.SetFloat(SpeedHash, normalized);
    }
}