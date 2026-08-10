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

    private bool fuelConsumptionStarted = false;
    private bool runStarted = true;
    private bool firstCash = true;
    private bool firstRepair = false;
    private bool firstkilled = false;
    private List<IWagon> wagons = new();
    private float timer = 6;

    private void Start()
    {
        CoalUi.SetActive(false);
        RunUi.SetActive(false);

        EventBus.Subscribe<OnSetAttackEnabledEvent>(SetAttackUi);
        EventBus.Subscribe<OnStartFuelUseEvent>(StartFuelConsumption);
        EventBus.Subscribe<OnStartSpawningEnemiesEvent>(StartRun);
        EventBus.Subscribe<OnEnemyKilledEvent>(CashGoldWagon);

        EventBus.Publish(new OnStartSpawningEnemiesEvent(false));
        EventBus.Publish(new OnSetCanConsumeEvent(false));
        EventBus.Publish(new OnSetTimerStartedEvent(false));
        EventBus.Publish(new OnEnableCoalBoxEvent(false));
        EventBus.Publish(new OnEnableGoldBoxEvent(false));
        EventBus.Publish(new OnSetAttackEnabledEvent(false));

        EventBus.Publish(new OnSetTutorialVisibleEvent(true));
        EventBus.Publish(new OnSetTutorialTextEvent("Bronco Buckle... back in the saddle again, huh? Let's go over the basics.\n<b>Press WASD to move</b>."));
    }
    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnStartFuelUseEvent>(StartFuelConsumption);
        EventBus.Unsubscribe<OnStartSpawningEnemiesEvent>(StartRun);
        EventBus.Unsubscribe<OnSetAttackEnabledEvent>(SetAttackUi);
        EventBus.Unsubscribe<OnEnemyKilledEvent>(CashGoldWagon);
    }
    private void Update()
    {
        timer -= 1 * Time.deltaTime;

        if (timer <= 0)
        {
            wagons.Add(RunManager.Instance.ActiveWagons[1]);

            EventBus.Publish(new OnSpawnEnemyEvent(EnemySpawn.position, wagons));

            if (!firstRepair)
            {
                firstRepair = true;
                EventBus.Publish(new OnSetTutorialVisibleEvent(true));
                EventBus.Publish(new OnSetTutorialTextEvent("That damn train you stole to reach the mysterious ore has every outlaw after you. \n <b>Repair the wagons with R!</b>"));
            }
            timer = float.MaxValue;
        }
    }

    void StartFuelConsumption(OnStartFuelUseEvent startFuelEvent)
    {
        if (!fuelConsumptionStarted)
        {
            CoalUi.SetActive(true);
            fuelConsumptionStarted = true;
            EventBus.Publish(new OnSetTutorialVisibleEvent(true));
            EventBus.Publish(new OnEnableCoalBoxEvent(true));
            EventBus.Publish(new OnSetTutorialTextEvent("One more thing, partner: your locomotive won't run on wishes. \n<b>Feed it coal, or the boiler's gonna blow!</b>"));
        }
    }

    void CashGoldWagon(OnEnemyKilledEvent enemyKillEvent)
    {
        if (firstCash)
        {
            runStarted = false;
            firstCash = false;
            EventBus.Publish(new OnSetTutorialVisibleEvent(true));
            EventBus.Publish(new OnEnableGoldBoxEvent(true));
            EventBus.Publish(new OnSetTutorialTextEvent("These fellas <b>burst into gold when they die</b>. The wagon can store it... but it ain't exactly safe. \n<b>Collect it and stash it in the safe.</b>"));
        }
    }

    void StartRun(OnStartSpawningEnemiesEvent enemiesStartEvent)
    {
        if (runStarted == false)
        {
            RunUi.SetActive(enemiesStartEvent.Can);
            EventBus.Publish(new OnSetCanConsumeEvent(true));
            EventBus.Publish(new OnSetTutorialVisibleEvent(true));
            EventBus.Publish(new OnSetTutorialTextEvent("Well, reckon that's all you need to know. <b>The road ahead is right here</b>. Good luck, Bronco Buckle!"));
            PlayerPrefs.SetInt("TutorialCompleted", 1);

            StartCoroutine(HideTextCoroutine(5f));
            runStarted = true;
        }
    }

    void SetAttackUi(OnSetAttackEnabledEvent setAttackEvent)
    {
        if (!firstkilled && setAttackEvent.Can)
        {
            firstkilled = true;
            EventBus.Publish(new OnSetTutorialVisibleEvent(true));
            EventBus.Publish(new OnSetTutorialTextEvent("Good. Surely you haven't forgotten how to <b>shoot</b>, right?"));
        }
    }

    private IEnumerator HideTextCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        EventBus.Publish(new OnSetTutorialVisibleEvent(false));
    }
}
