using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerBrain brain;
    [SerializeField] private Rigidbody rb;

    [Header("Locomotion")]
    [Tooltip("Por debajo de este Speed (0-1) se mantiene la última dirección en vez de recalcularla, " +
             "así el blend tree no tiembla cuando el jugador frena.")]
    [SerializeField, Range(0f, 0.3f)] private float directionMinSpeed = 0.05f;
    [Tooltip("Suavizado del cambio de dirección (MoveX/MoveY). 0 = sin suavizado.")]
    [SerializeField, Min(0f)] private float directionDampTime = 0.06f;

    [Header("Weapon overrides")]
    [Tooltip("Un Animator Override Controller por tipo de arma, en el mismo orden que el enum (índice = valor del enum). " +
             "Todos deben apuntar al mismo controller base que tiene el Animator.")]
    [SerializeField] private AnimatorOverrideController[] weaponOverrides;

    private float maxSpeed;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int IsRepairingHash = Animator.StringToHash("IsRepairing");

    private AnimatorOverrideController runtimeOverride;
    private readonly List<KeyValuePair<AnimationClip, AnimationClip>> runtimePairs = new();
    private readonly List<KeyValuePair<AnimationClip, AnimationClip>> sourcePairs = new();
    private readonly Dictionary<AnimationClip, AnimationClip> lookup = new();

    private bool lastRepairing;
    private Vector2 lastDirection = Vector2.up; // por defecto: hacia adelante

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (brain == null) brain = GetComponent<PlayerBrain>();
        if (rb == null) rb = GetComponent<Rigidbody>();

        maxSpeed = brain.Speed;

        // Una única instancia de override: se le cambian los clips sin reasignar el controller,
        // así el Animator no reinicia su state machine ni sus parámetros.
        runtimeOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = runtimeOverride;

        EventBus.Subscribe<OnWeaponChangedEvent>(HandleWeaponChanged);
    }

    private void Start()
    {
        // Aplica el arma con la que arranca el jugador.
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
        UpdateLocomotion();
    }

    #region Weapon

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

    #endregion

    #region Parámetros

    private void UpdateRepairing()
    {
        bool repairing = brain.IsRepairing;
        if (repairing == lastRepairing) return;

        lastRepairing = repairing;
        animator.SetBool(IsRepairingHash, repairing);
    }

    private void UpdateLocomotion()
    {
        float normalizedSpeed = 0f;
        Vector3 worldVelocity = Vector3.zero;

        if (!brain.IsRepairing && rb != null && maxSpeed > 0f)
        {
            worldVelocity = rb.linearVelocity;
            worldVelocity.y = 0f;
            normalizedSpeed = Mathf.Clamp01(worldVelocity.magnitude / maxSpeed);
        }

        animator.SetFloat(SpeedHash, normalizedSpeed);

        // Dirección del movimiento relativa a hacia dónde mira el player (mouse).
        // x = lateral (derecha +), y = adelante/atrás (adelante +).
        if (normalizedSpeed >= directionMinSpeed)
        {
            Vector3 local = transform.InverseTransformDirection(worldVelocity.normalized);
            lastDirection = new Vector2(local.x, local.z);
        }

        if (directionDampTime > 0f)
        {
            animator.SetFloat(MoveXHash, lastDirection.x, directionDampTime, Time.deltaTime);
            animator.SetFloat(MoveYHash, lastDirection.y, directionDampTime, Time.deltaTime);
        }
        else
        {
            animator.SetFloat(MoveXHash, lastDirection.x);
            animator.SetFloat(MoveYHash, lastDirection.y);
        }
    }

    #endregion
}