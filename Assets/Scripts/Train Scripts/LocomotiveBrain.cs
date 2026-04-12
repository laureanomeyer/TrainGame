using UnityEngine;
using UnityEngine.SceneManagement;

public class LocomotiveBrain : MonoBehaviour, IDamagable, IWagon
{
    public LocomotiveFuel fuelController;
    private TrainData dataRef;
    [SerializeField] private Material materialDeVagonDestruido;
    [SerializeField] private Renderer rendererWagon;
    [SerializeField] public Transform TailRef;

    public float CurrentShield => fuelController.CurrentShield;
    public float MaxShield => fuelController.MaxShield;
    public Transform Transform => transform;

    void Start()
    {
        dataRef = RunManager.Instance.TrainCopyData;
        fuelController = new LocomotiveFuel(dataRef.stats.shields * 2, dataRef.stats.trainMaxHp * 10, dataRef.stats.baseSpeed, dataRef.stats.shields, dataRef.stats.fuelOptimizer);
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

    public void AddTrainStats( TrainData newStats)
    {
        dataRef.stats += newStats.stats;
    }

}
