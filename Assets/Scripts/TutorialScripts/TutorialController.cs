using UnityEngine;
using static TrainStats;
using System.Collections;

public class TutorialController : MonoBehaviour
{
    [SerializeField] GameObject Ui;
    float timer = 5;
    int source = 1;

    private void Awake()
    {
        Ui.SetActive(false);
        GameManager.Instance.Session.StatSystem.AddModifier(new StatModifier(StatType.FuelOptimizer, 1000, ModifierType.Multipicaive, source));
        GameManager.Instance.Session.StatSystem.RecalculateAll();
        TutorialEvents.OnStartFuelUse += StartFuelConsumption;
    }
    private void Update()
    {
        timer -= 1 * Time.deltaTime;

        if (timer <= 0)
        {
            TutorialEvents.SpawnEnemy();
            timer = 100000;
        }
    }

    void StartFuelConsumption()
    {
        GameManager.Instance.Session.StatSystem.RemoveModifiersFromSource(source);
        GameManager.Instance.Session.StatSystem.RecalculateAll();
        Ui.SetActive(true);
    }

    private void OnDestroy()
    {
        TutorialEvents.OnStartFuelUse -= StartFuelConsumption;
    }
}
