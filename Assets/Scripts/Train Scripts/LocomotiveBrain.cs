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

    public LocomotiveFuel fuelController;
    private TrainData dataRef;

    public float CurrentShield => fuelController.CurrentShield;
    public float MaxShield => fuelController.MaxShield;
    public Transform Transform => transform;

    void Start()
    {
        dataRef = RunManager.Instance.TrainCopyData;
        var stats = RunManager.Instance.StatSystem;

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
        if(fuelController.CurrentMaxFuel >= 0)
        {
            rendererWagon.material = materialDeVagonDestruido;
            SceneManager.LoadScene("LauScene");
        }
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
