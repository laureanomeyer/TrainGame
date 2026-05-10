using UnityEngine;
using UnityEngine.SceneManagement;

public class LocomotiveBrain : MonoBehaviour, IDamagable, IWagon
{
    [SerializeField] private Material materialDeVagonDestruido;
    [SerializeField] private Renderer rendererWagon;
    [SerializeField] public Transform TailRef;

    [SerializeField] private float CM;
    [SerializeField] private float EM;
    [SerializeField] private float RES;

    private bool destroyed;

    public LocomotiveFuel fuelController;
    private TrainData dataRef;
    private DamageFlash flash;
    private Animator animator;

    public float CurrentShield => fuelController.CurrentShield;
    public float MaxShield => fuelController.MaxShield;
    public Transform Transform => transform;

    void Start()
    {
        dataRef = GameManager.Instance.Session.TrainData;
        var stats = RunManager.Instance.StatSystem;
        flash = GetComponent<DamageFlash>();
        animator = GetComponent<Animator>();

        fuelController = new LocomotiveFuel(
            EM * stats.GetStat(StatType.Defense),
            CM * stats.GetStat(StatType.MaxHp),
            dataRef.LocomotiveStatsMultiplicator.baseSpeed,
            RES * stats.GetStat(StatType.Defense),
            dataRef.LocomotiveStatsMultiplicator.fuelOptimizer
            );
        stats.OnStatChanged += OnStatChanged;
    }

    void Update()
    {
        fuelController.Move(Time.deltaTime);
        fuelController.UpdateShield(Time.deltaTime);
    }

    public void TakeDamage(float damageAmount)
    {
        fuelController.TakeDamage(damageAmount);
        
        if (flash != null)
        {
            flash.Flash();
        }
        if (animator != null) animator.SetTrigger("Damage");
        
    }

    public void AddFuel()
    {
        fuelController.AddFuel();
    }

    public void Repair(float repairAmount)
    {
    }

    public void Break()
    {
        if (destroyed) return;

        destroyed = true;

        if (rendererWagon != null && materialDeVagonDestruido != null)
        {
            rendererWagon.material = materialDeVagonDestruido;
        }

        GameManager.Instance.Defeat();
    }
    private void OnStatChanged(StatType type, float newValue)
    {
    }

    private void OnDestroy()
    {
        if (RunManager.Instance != null)
            RunManager.Instance.StatSystem.OnStatChanged -= OnStatChanged;
    }
}
