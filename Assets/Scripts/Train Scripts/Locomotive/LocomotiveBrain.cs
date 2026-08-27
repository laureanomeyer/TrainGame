using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;

public class LocomotiveBrain : MonoBehaviour, IDamagable, IWagon
{
    [SerializeField] public Transform TailRef;

    [SerializeField] private Transform coalBox;

    [SerializeField] private float CM;
    [SerializeField] private float EM;
    [SerializeField] private float RES;

    [SerializeField] private Renderer shieldsRenderer;

    [SerializeField] private VisualEffect[] explosionParticles;

    [Header("UI")]

    [SerializeField] private TextMeshProUGUI currentCoalUI;

    [Header("Cinematic")]
    [SerializeField] private string locomotiveAnchorKey = "Locomotive";

    [Header("Top Locomotive Render")]
    [SerializeField] public Renderer locomotiveTopRender;
    [SerializeField] public MeshFilter locomotiveTopMeshFilter;

    private LocomotiveRenderController renderController;
    private ICinematicActorRegistry cinematicRegistry;

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

        //Build up particles + Delay based on distance

        StartCoroutine(ExplotionDelay());

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

    private IEnumerator ExplotionDelay()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        if (explosionParticles == null || explosionParticles.Length == 0)
        {
            Debug.LogWarning("explosionParticles vac�o o sin asignar en " + gameObject.name);
            yield break;
        }

        foreach (var vfx in explosionParticles)
        {
            if (vfx != null)
            {
                vfx.gameObject.SetActive(true);
                vfx.Play();
                AudioManager.Instance.Play("SFXExplosionBoom");
            }

            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}