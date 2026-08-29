using TMPro;
using UnityEngine;

public class LocomotiveBrain : MonoBehaviour, IDamagable, IWagon
{
    [SerializeField] public Transform TailRef;

    [SerializeField] private Transform coalBox;

    [SerializeField] private float CM;
    [SerializeField] private float EM;
    [SerializeField] private float RES;

    [SerializeField] private Renderer shieldsRenderer;

    [Header("UI")]

    [SerializeField] private TextMeshProUGUI currentCoalUI;

    [Header("Cinematic")]
    [SerializeField] private string locomotiveAnchorKey = "Locomotive";

    [Header("Top Locomotive Render")]
    [SerializeField] public Renderer locomotiveTopRender;
    [SerializeField] public MeshFilter locomotiveTopMeshFilter;

    private LocomotiveRenderController renderController;
    private ICinematicActorRegistry cinematicRegistry;
    private ParticleSequenceController particleSequence;

    private bool destroyed;
    public LocomotiveFuel fuelController;
    private DamageFlash flash;
    private Animator animator;
    private StatSystem stats;
    private bool started = false;

    private CoalCollector coalCollector;
    public CoalCollector CoalCollector => coalCollector;

    public float CurrentShield => fuelController.CurrentShield;
    public float MaxShield => fuelController.MaxShield;
    public Transform Transform => transform;

    void Awake()
    {
        var dataRef = ServiceLocator.Get<TrainData>();
        coalCollector = new CoalCollector(currentCoalUI);
        dataRef.SetCoalBox(coalBox);
        stats = RunManager.Instance.StatSystem;
        flash = GetComponent<DamageFlash>();
        animator = GetComponent<Animator>();
        particleSequence = GetComponent<ParticleSequenceController>();
        fuelController = new LocomotiveFuel(
            EM * stats.GetStat(StatType.Defense),
            CM * stats.GetStat(StatType.MaxHp),
            RES * stats.GetStat(StatType.Defense),
            stats.GetStat(StatType.FuelOptimizer),
            shieldsRenderer
        );


        renderController = new LocomotiveRenderController(this);

        fuelController.OnDestroyed += Break;
        stats.OnStatChanged += OnStatChanged;
        EventBus.Subscribe<OnStartFuelUseEvent>(RemoveFuelTutorial);

        renderController.ForceDeactivateTop();

        cinematicRegistry = ServiceLocator.Get<ICinematicActorRegistry>();
        cinematicRegistry?.RegisterDynamic(locomotiveAnchorKey, transform);
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

        cinematicRegistry?.UnregisterDynamic(locomotiveAnchorKey);
    }

    void Update()
    {
        if (destroyed) return;

        fuelController.Move(Time.deltaTime);
        fuelController.UpdateShield(Time.deltaTime);
    }

    public void TakeDamage(float damageAmount)
    {
        if (destroyed) return;
        if (!GameManager.Instance.IsGameplayState) return;

        fuelController.TakeDamage(damageAmount);

        if (flash != null)
            flash.Flash();

        if (animator != null)
            animator.SetTrigger("Damage");
    }

    public void Break()
    {
        if (destroyed) return;
        destroyed = true;

        AudioManager.Instance.Play("SFXExplosionBuildUp");

        particleSequence?.PlayGroup("explosion");

        EventBus.Publish(new OnRunEndedEvent(RunResult.Defeat));
    }

    void RemoveFuel()
    {
        fuelController.RemoveFuel(CM * stats.GetStat(StatType.MaxHp) / 1.5f);
    }

    void RemoveFuelTutorial(OnStartFuelUseEvent startFuelEvent)
    {
        if (started) return;

        started = true;
        fuelController.RemoveFuel(CM * stats.GetStat(StatType.MaxHp) / 1.5f);
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
}