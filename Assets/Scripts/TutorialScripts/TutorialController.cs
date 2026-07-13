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

    private bool started = false;
    private bool firstCash = true;
    private bool firstRepair = false;
    private bool firstkilled = false;
    private List<IWagon> wagons = new();
    private float timer = 6;

    private void Start()
    {
        CoalUi.SetActive(false);
        RunUi.SetActive(false);

        TutorialEvents.OnSetAttackEnabled += SetAttackUi;
        TutorialEvents.OnStartFuelUse += StartFuelConsumption;
        TutorialEvents.OnStartSpawningEnemies += StartRun;
        TutorialEvents.OnEnemyKilled += CashGoldWagon;

        TutorialEvents.SetRunStarted(false);
        TutorialEvents.SetCanConsume(false);
        TutorialEvents.SetTimerStarted(false);
        TutorialEvents.EnableCoalBox(false);
        TutorialEvents.EnableGoldBox(false);
        TutorialEvents.SetAttackEnabled(false);

        TutorialEvents.SetTutorialTextVisible(true);
        TutorialEvents.SetTutorialText("Bronco Buckle... back in the saddle again, huh? Let's go over the basics.\n<b>Press WASD to move</b>.");
    }
    private void Update()
    {
        timer -= 1 * Time.deltaTime;

        if (timer <= 0)
        {
            wagons.Add(RunManager.Instance.ActiveWagons[1]);

            TutorialEvents.SpawnEnemy(EnemySpawn.position, wagons);


            if (!firstRepair)
            {
                firstRepair = true;
                TutorialEvents.SetTutorialTextVisible(true);
                TutorialEvents.SetTutorialText("That damn train you stole to reach the mysterious ore has every outlaw after you. \n <b>Repair the wagons with R!</b>");
            }
            timer = float.MaxValue;
        }
    }

    void StartFuelConsumption()
    {
        if (!started)
        {
            CoalUi.SetActive(true);
            started = true;
            TutorialEvents.SetTutorialTextVisible(true);
            TutorialEvents.EnableCoalBox(true);
            TutorialEvents.SetTutorialText("One more thing, partner: your locomotive won't run on wishes. \n<b>Feed it coal, or the boiler's gonna blow!</b>");
        }
    }

    void CashGoldWagon()
    {
        if (firstCash)
        {
            firstCash = false;
            TutorialEvents.SetTutorialTextVisible(true);
            TutorialEvents.EnableGoldBox(true);
            TutorialEvents.SetTutorialText("These fellas <b>burst into gold when they die</b>. The wagon can store it... but it ain't exactly safe. \n<b>Collect it and stash it in the safe.</b>");
        }
    }

    void StartRun(bool can)
    {
        RunUi.SetActive(can);
        TutorialEvents.SetCanConsume(true);
        TutorialEvents.SetTutorialTextVisible(true);
        TutorialEvents.SetTutorialText("Well, reckon that's all you need to know. <b>The road ahead is right here</b>. Good luck, Bronco Buckle!");
    }

    void SetAttackUi(bool show)
    {

        if (!firstkilled && show)
        {
            firstkilled = true;
            TutorialEvents.SetTutorialTextVisible(true);
            TutorialEvents.SetTutorialText("Good. Surely you haven't forgotten how to <b>shoot</b>, right?");
        }
    }

    private void OnDestroy()
    {
        TutorialEvents.OnStartFuelUse -= StartFuelConsumption;
        TutorialEvents.OnStartSpawningEnemies -= StartRun;
        TutorialEvents.OnSetAttackEnabled -= SetAttackUi;
        TutorialEvents.OnEnemyKilled -= CashGoldWagon;
    }
}
