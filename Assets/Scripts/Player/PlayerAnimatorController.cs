using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerBrain brain;
    [SerializeField] private Rigidbody rb;


    private float maxSpeed;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsRepairingHash = Animator.StringToHash("IsRepairing");

    private bool lastRepairing;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (brain == null) brain = GetComponent<PlayerBrain>();
        if (rb == null) rb = GetComponent<Rigidbody>();

        maxSpeed = brain.Speed;
    }

    private void Update()
    {
        if (animator == null || brain == null) return;

        UpdateRepairing();
        UpdateSpeed();
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