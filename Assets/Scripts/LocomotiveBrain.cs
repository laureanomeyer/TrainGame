using UnityEngine;
using UnityEngine.SceneManagement;

public class LocomotiveBrain : MonoBehaviour
{
    [SerializeField] private LocomotiveStats stats;
    public LocomotiveFuel fuelController;

    [SerializeField] private Material materialDeVagonDestruido;
    [SerializeField] private Renderer rendererWagon;

    public float CurrentShield => fuelController.CurrentShield;
    public float MaxShield => fuelController.MaxShield;

    void Start()
    {
        fuelController = new LocomotiveFuel(stats.defense * 2, stats.maxFuel, stats.baseSpeed, stats.defense);
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
        fuelController.AddFuel(stats.maxFuel);
    }

    public void Repair(float repairAmount)
    {
        return;
    }

    public void BreakDown()
    {
        rendererWagon.material = materialDeVagonDestruido;
        SceneManager.LoadScene("LauScene");
    }
}
