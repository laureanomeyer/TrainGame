using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class TutorialController : MonoBehaviour
{
    [SerializeField] GameObject CoalUi;
    [SerializeField] GameObject RunUi;
    [SerializeField] GameObject attackCursor;
    [SerializeField] Transform EnemySpawn;
    [SerializeField] private EnemyData data;

    private bool fuelConsumptionStarted = false;
    private bool runStarted = true;
    private bool firstCash = true;
    private bool firstRepair = false;
    private bool firstkilled = false;
    private List<IWagon> wagons = new();
    private float timer = 1;

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

            EventBus.Publish(new OnSpawnEnemyEvent(EnemySpawn.position, data));

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

public class TutorialCOntroller : MonoBehaviour
{
    [SerializeField] private string[] texts;
    private int currentStep = 0;

    [SerializeField] private CanvasGroup darkerFilter;
    [SerializeField] private TextMeshProUGUI tutorialText;

    [SerializeField] private CanvasGroup fuelUi;
    [SerializeField] private CanvasGroup shieldsUi;
    [SerializeField] protected CanvasGroup goldHpUi;

    [SerializeField] private float holdDuration = 1f;


    private void Awake()
    {
        tutorialText.text = texts[currentStep];

        fuelUi.alpha = 0f;
        shieldsUi.alpha = 0f;
        goldHpUi.alpha = 0f;

        EventBus.Subscribe<OnStartFuelUseEvent>(StartFuelConsumption);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnStartFuelUseEvent>(StartFuelConsumption);
    }

    private void Start()
    {
        EventBus.Publish(new OnFreezePlayerEvent(false));
    }
    void StartFuelConsumption(OnStartFuelUseEvent startFuelEvent)
    {
        fuelUi.alpha = 1f;
        EventBus.Publish(new OnEnableCoalBoxEvent(true));
    }

    private void AdvanceStep()
    {
        currentStep += 1;
        tutorialText.text = texts[currentStep];
        HandleSteps(currentStep);
    }
    private void HandleSteps(int step)
    {
        switch (step)
        {
            case 0: 
                break;
            case 1:
                EventBus.Publish(new OnStartFuelUseEvent());
                break;
            case 2:
                break;
            case 3:
                EventBus.Publish(new OnShowCoalWaypointEvent());
                break;
            case 4:
                break;
            case 5:
                //Spawnear un carbonero
                break;
            case 6:
                //Congelar al jugador de nuevo y poner texto
                break;
            case 7:
                //Spawnear enemigo comun y descongelar al player
                break;
            case 8:
                //Congelar al player y poner texto, el enemigo dispara unas veces y baja la barra de maximo
                break;
            case 9:
                //Aparece el escudo, el enemigo dispara y recibe daño el escudo, RECIBIR DAÑO AVANZA INSTANTANEAMENTE AL 10
                shieldsUi.alpha = 1f;
                break;
            case 10:
                //El enemigo deja de disparar, el escudo se regenera, una vez al maximo AVANZA DIRECTAMENTE AL 11
                break;
            case 11:
                //Aparece la vida del vagon de oro baja, el enemigo dispara de nuevo y lo rompe
                goldHpUi.alpha = 1f;
                break;
            case 12:
                //Se descongela al player, aparece la ui de reparar sobre el vagon de oro, se mata al enemigo. Al llegar el oro a la caja se avanza al 13
                break;
            case 13:
                //Se congela al player
                break;
            case 14: 
                //Se descongela al player y aparece la Ui de oro sobre la bolsa, se recoje y se deposita, al depositar SE AVANZA AL 15
                break;
            case 15:
                //Se prende la Ui de oro total
                break;
            case 16:
                //Texto sobre la tienda
                break;
            case 17:
                //Texto final, Empezar la run
                break;
            default:
                break;





        }
    }
    private IEnumerator HoldCoroutine(float holdTime)
    {
        yield return new WaitForSeconds(holdTime);
        currentStep += 1;
        tutorialText.text = texts[currentStep];
    }
}
