using UnityEngine;
using static TrainStats;
using System.Collections;

public class TutorialController : MonoBehaviour
{
    [SerializeField] GameObject CoalUi;
    [SerializeField] GameObject RunUi;
    float timer = 5;

    private void Awake()
    {
        TutorialEvents.SetRunStarted(false);
    }
    private void Start()
    {
        CoalUi.SetActive(false);
        RunUi.SetActive(false);
        TutorialEvents.SetCanConsume(false);
        TutorialEvents.SetTimerStarted(false);
        TutorialEvents.OnStartFuelUse += StartFuelConsumption;
        TutorialEvents.OnSetRunStarted += StartRun;
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
        TutorialEvents.SetCanConsume(true);
        CoalUi.SetActive(true);
    }

    void StartRun(bool c)
    {
        RunUi.SetActive(c);
    }

    private void OnDestroy()
    {
        TutorialEvents.OnStartFuelUse -= StartFuelConsumption;
        TutorialEvents.OnSetRunStarted -= StartRun;
    }
}
