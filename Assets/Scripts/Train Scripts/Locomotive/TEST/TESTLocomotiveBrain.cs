using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class TESTLocomotiveBrain : MonoBehaviour
{

    [SerializeField] public Transform TailRef;

    [SerializeField] private float CM;
    [SerializeField] private float EM;
    [SerializeField] private float RES;

    [SerializeField] private Renderer shieldsRenderer;

    [Header("Top Locomotive Render")]
    [SerializeField] public Renderer locomotiveTopRender;
    [SerializeField] public MeshFilter locomotiveTopMeshFilter;


    [SerializeField] private float explosionDelayPerUnit = 0.1f;

    private TESTRenderController renderController;
    private ICinematicActorRegistry cinematicRegistry;

    private ParticleSequenceController particleSequenceController;

    private bool destroyed;
    public TESTLocomotiveFuel fuelController;
    private DamageFlash flash;
    private Animator animator;
    private bool started = false;

    public float CurrentShield => fuelController.CurrentShield;
    public float MaxShield => fuelController.MaxShield;
    public Transform Transform => transform;


    void Start()
    {
        flash = GetComponent<DamageFlash>();
        animator = GetComponent<Animator>();
        particleSequenceController = GetComponent<ParticleSequenceController>();

        fuelController = new TESTLocomotiveFuel(
            EM ,
            CM ,
            RES ,
            1,
            shieldsRenderer
        );

        renderController = new TESTRenderController(this);

        fuelController.OnDestroyed += Break;
        EventBus.Subscribe<OnStartFuelUseEvent>(RemoveFuelTutorial);

        renderController.ForceDeactivateTop();
    }

    private void OnDestroy()
    {
        if (RunManager.Instance != null)
            RunManager.Instance.StatSystem.OnStatChanged -= OnStatChanged;

        if (fuelController != null)
        {
            fuelController.OnDestroyed -= Break;
            fuelController.Destroy();
        }

        EventBus.Unsubscribe<OnStartFuelUseEvent>(RemoveFuelTutorial);
    }

    public void TakeDamage(float damageAmount)
    {
        if (destroyed) return;


        fuelController.TakeDamage(damageAmount);

        if (flash != null)
            flash.Flash();

        if (animator != null)
            animator.SetTrigger("Damage");
    }

    [ContextMenu("Break the Locomotive")]
    public void Break()
    {
        if (particleSequenceController == null)
        {
            Debug.LogWarning($"[TESTLocomotiveBrain] particleSequenceController no asignado en {gameObject.name}.", this);
            return;
        }

        particleSequenceController.PlayGroup("Vapor");
        StartCoroutine(ExplotionDelay());
    }

    void RemoveFuel()
    {
        fuelController.RemoveFuel(CM  / 1.5f);
    }

    void RemoveFuelTutorial(OnStartFuelUseEvent startFuelEvent)
    {
        if (started) return;

        started = true;
        fuelController.RemoveFuel(CM  / 1.5f);
    }

    public void AddFuel()
    {
        fuelController.AddFuel();
    }

    public void Repair(float repairAmount) { }

    private void OnStatChanged(StatType type, float newValue) { }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            renderController.DeactivateWagonTop();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            renderController.ActivateWagonTop();
        }
    }

    private IEnumerator ExplotionDelay()
    {
        float distance = TailRef != null ? Vector3.Distance(transform.position, TailRef.position) : 0f;
        float delay = distance * explosionDelayPerUnit;

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        particleSequenceController.PlayGroup("Explosion");
    }
}
