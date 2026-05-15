using UnityEngine;
using static TrainStats;
using System.Collections;
using System.Collections.Generic;

public class TutorialController : MonoBehaviour
{
    [SerializeField] GameObject CoalUi;
    [SerializeField] GameObject RunUi;
    [SerializeField] GameObject attackCursor;
    [SerializeField] Transform EnemySpawn;
    private List<IWagon> wagons = new();
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
        TutorialEvents.OnSetAttackEnabled += SetAttackUi;
        TutorialEvents.SetAttackEnabled(false);
        TutorialEvents.OnStartFuelUse += StartFuelConsumption;
        TutorialEvents.OnStartSpawningEnemies += StartRun;
    }
    private void Update()
    {
        timer -= 1 * Time.deltaTime;

        if (timer <= 0)
        {
            wagons.Add(RunManager.Instance.ActiveWagons[1]);
            TutorialEvents.SpawnEnemy(EnemySpawn.position, wagons);
            timer = 10000000000000;
        }
    }

    void StartFuelConsumption()
    {
        TutorialEvents.SetCanConsume(true);
        CoalUi.SetActive(true);
    }

    void StartRun(bool can)
    {
        RunUi.SetActive(can);
    }

    void SetAttackUi(bool show)
    {
        Cursor.visible = show;
        attackCursor.SetActive(show);
    }

    private void OnDestroy()
    {
        TutorialEvents.OnStartFuelUse -= StartFuelConsumption;
        TutorialEvents.OnStartSpawningEnemies -= StartRun;
        TutorialEvents.OnSetAttackEnabled -= SetAttackUi;
    }
}
