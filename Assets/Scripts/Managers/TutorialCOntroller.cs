using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialCOntroller : MonoBehaviour
{
    [SerializeField] private string[] texts;
    [SerializeField] private int currentStep = 0;

    [SerializeField] private CanvasGroup darkerFilter;
    [SerializeField] private TextMeshProUGUI tutorialText;

    [SerializeField] private CanvasGroup fuelUi;
    [SerializeField] private CanvasGroup shieldsUi;
    [SerializeField] private CanvasGroup goldHpUi;
    [SerializeField] private CanvasGroup goldAmountUi;

    [SerializeField] private float holdDuration = 1f;

    [SerializeField] Transform EnemySpawn;
    [SerializeField] private EnemyData coalEnemy;
    [SerializeField] private EnemyData commonEnemy;

    private bool playerFrozen;


    private void Awake()
    {
        tutorialText.text = texts[currentStep];

        fuelUi.alpha = 0f;
        shieldsUi.alpha = 0f;
        goldHpUi.alpha = 0f;

        EventBus.Subscribe<OnStartFuelUseEvent>(StartFuelConsumption);
        EventBus.Subscribe<OnFreezePlayerEvent>(SetPlayerFrozen);
        EventBus.Subscribe<OnForceTutorialStepEvent>(ForceStep);
        EventBus.Subscribe<OnAdvanceTutorialStepByClick>(AdvanceStepByClicking);
        EventBus.Subscribe<OnAdvanceTutorialStep>(AdvanceStepNaturally);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnStartFuelUseEvent>(StartFuelConsumption);
        EventBus.Unsubscribe<OnFreezePlayerEvent>(SetPlayerFrozen);
        EventBus.Unsubscribe<OnForceTutorialStepEvent>(ForceStep);
        EventBus.Unsubscribe<OnAdvanceTutorialStepByClick>(AdvanceStepByClicking);
        EventBus.Unsubscribe<OnAdvanceTutorialStep>(AdvanceStepNaturally);
    }

    private void Start()
    {
        fuelUi.alpha = 0f;
        shieldsUi.alpha = 0f;
        goldHpUi.alpha = 0f;
        goldAmountUi.alpha = 0f;

        EventBus.Publish(new OnFreezePlayerEvent(false));
        EventBus.Publish(new OnSetShieldsActiveEvent(false));
        EventBus.Publish(new OnStartSpawningEnemiesEvent(false));
        EventBus.Publish(new OnSetCanConsumeEvent(false));
        EventBus.Publish(new OnSetTimerStartedEvent(false));
        EventBus.Publish(new OnEnableCoalBoxEvent(false));
        EventBus.Publish(new OnEnableGoldBoxEvent(false));
        playerFrozen = true;
    }
    void StartFuelConsumption(OnStartFuelUseEvent startFuelEvent)
    {
        fuelUi.alpha = 1f;
        EventBus.Publish(new OnEnableCoalBoxEvent(true));
    }

    private void SetPlayerFrozen(OnFreezePlayerEvent ev)
    {
        playerFrozen = !ev.Activated;
    }

    private void AdvanceStepByClicking(OnAdvanceTutorialStepByClick ev)
    {
        if (!playerFrozen) return;

        currentStep += 1;
        if ( currentStep >= texts.Length)
        {
            return;
        }
        tutorialText.text = texts[currentStep];
        HandleSteps(currentStep);
    }
    private void AdvanceStepNaturally(OnAdvanceTutorialStep ev)
    {
        currentStep += 1;
        tutorialText.text = texts[currentStep];
        HandleSteps(currentStep);
    }

    private void ForceStep(OnForceTutorialStepEvent ev)
    {
        currentStep = ev.number;
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
                EventBus.Publish(new OnFreezePlayerEvent(true));
                break;
            case 4:
                EventBus.Publish(new OnFreezePlayerEvent(true));
                break;
            case 5:
                EventBus.Publish(new OnSpawnEnemyEvent(EnemySpawn.position, coalEnemy));
                break;
            case 6:
                EventBus.Publish(new OnFreezePlayerEvent(false));
                break;
            case 7:
                EventBus.Publish(new OnSpawnEnemyEvent(EnemySpawn.position, commonEnemy));
                EventBus.Publish(new OnFreezePlayerEvent(true));
                break;
            case 8:
                //Congelar al player y poner texto, el enemigo dispara unas veces y baja la barra de maximo
                EventBus.Publish(new OnFreezePlayerEvent(false));
                EventBus.Publish(new OnSetTutorialEnemyTarget(0));
                break;
            case 9:
                //Aparece el escudo, el enemigo dispara y recibe daño el escudo, RECIBIR DAÑO AVANZA INSTANTANEAMENTE AL 10
                //Evento de activar el escudo
                EventBus.Publish(new OnSetShieldsActiveEvent(true));
                shieldsUi.alpha = 1f;
                break;
            case 10:
                //El enemigo deja de disparar, el escudo se regenera, una vez al maximo AVANZA DIRECTAMENTE AL 11
                //Evento de enemigo deja de disparar
                break;
            case 11:
                //Aparece la vida del vagon de oro baja, el enemigo dispara de nuevo y lo rompe
                EventBus.Publish(new OnSetTutorialEnemyTarget(1));
                goldHpUi.alpha = 1f;
                EventBus.Publish(new OnActivateGoldWagon());
                break;
            case 12:
                //Se descongela al player, aparece la ui de reparar sobre el vagon de oro, se mata al enemigo. Al llegar el oro a la caja se avanza al 13
                EventBus.Publish(new OnFreezePlayerEvent(true));
                break;
            case 13:
                //Se congela al player, el texto explica que no esta a salvo
                EventBus.Publish(new OnFreezePlayerEvent(false));
                break;
            case 14:
                //Se descongela al player y aparece la Ui de oro sobre la bolsa, se recoje y se deposita, al depositar SE AVANZA AL 15
                EventBus.Publish(new OnFreezePlayerEvent(true));
                break;
            case 15:
                //Se prende la Ui de oro total
                goldAmountUi.alpha = 1f;
                break;
            case 16:
                //Texto sobre la tienda
                break;
            case 17:
                //Texto final, Empezar la run
                EventBus.Publish(new OnSetCanConsumeEvent(true));
                PlayerPrefs.SetInt("TutorialCompleted", 1);
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