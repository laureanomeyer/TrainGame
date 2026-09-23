using System.Collections;
using TMPro;
using UnityEngine;

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
        goldAmountUi.alpha = 0f;

        EventBus.Subscribe<OnStartFuelUseEvent>(StartFuelConsumption);
        EventBus.Subscribe<OnFreezePlayerEvent>(SetPlayerFrozen);
        EventBus.Subscribe<OnAdvanceTutorialStepByClick>(AdvanceStepByClicking);
        EventBus.Subscribe<OnAdvanceTutorialStep>(AdvanceStepNaturally);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnStartFuelUseEvent>(StartFuelConsumption);
        EventBus.Unsubscribe<OnFreezePlayerEvent>(SetPlayerFrozen);
        EventBus.Unsubscribe<OnAdvanceTutorialStepByClick>(AdvanceStepByClicking);
        EventBus.Unsubscribe<OnAdvanceTutorialStep>(AdvanceStepNaturally);
    }

    private void Start()
    {
        EventBus.Publish(new OnSetAttackEnabledEvent(false));
        EventBus.Publish(new OnFreezePlayerEvent(false));
        EventBus.Publish(new OnSetShieldsActiveEvent(false));
        EventBus.Publish(new OnEnableGoldBoxEvent(false));
        EventBus.Publish(new OnStartSpawningEnemiesEvent(false));
        EventBus.Publish(new OnSetCanConsumeEvent(false));
        EventBus.Publish(new OnSetTimerStartedEvent(false));
        EventBus.Publish(new OnSetTutorialVisibleEvent(true));
    }
    void StartFuelConsumption(OnStartFuelUseEvent startFuelEvent)
    {
        fuelUi.alpha = 1f;
    }

    private void SetPlayerFrozen(OnFreezePlayerEvent ev)
    {
        playerFrozen = !ev.Activated;

        if (playerFrozen)
        {
            darkerFilter.alpha = 1f;
        }
        else darkerFilter.alpha = 0f;
    }

    private void HandleSteps(int step)
    {
        switch (step)
        {
            case 0:
                EventBus.Publish(new OnSetAttackEnabledEvent(false));
                //The train is your only way to get through the road, you gotta protect it
                break;
            case 1:
                //This dial shows the remaining fuel
                EventBus.Publish(new OnStartFuelUseEvent());
                break;
            case 2:
                //Make sure you don�t run out of it, otherwise the machine will explode
                break;
            case 3:
                //Try replenishing the fuel
                EventBus.Publish(new OnEnableCoalBoxEvent(true));
                EventBus.Publish(new OnCoalEarnedEvent(1f));
                break;
            case 4:
                //El player recarga fuel y se avanza al 5, te quedaste sin reservas
                EventBus.Publish(new OnFreezePlayerEvent(true));
                break;
            case 5:
                //Your fuel has been replenished but you ran out of reserves
                EventBus.Publish(new OnFreezePlayerEvent(false));
                break;
            case 6:
                //Some enemies will drop coal when they�re killed
                EventBus.Publish(new OnSpawnEnemyEvent(EnemySpawn.position, coalEnemy));
                EventBus.Publish(new OnFreezePlayerEvent(true));
                EventBus.Publish(new OnSetAttackEnabledEvent(true));
                break;
            case 7:
                EventBus.Publish(new OnFreezePlayerEvent(false));
                EventBus.Publish(new OnSetAttackEnabledEvent(false));
                break;
            case 8:
                //However, others will be more hostile
                EventBus.Publish(new OnSpawnEnemyEvent(EnemySpawn.position, commonEnemy));
                EventBus.Publish(new OnSetTutorialEnemyTarget(0));
                break;
            case 9:
                //Congelar al player y poner texto, el enemigo dispara unas veces y baja la barra de maximo
                //Damage to the hull will determine your maximun fuel capacity and it can�t be restored until you reach a station
                break;
            case 10:
                //Aparece el escudo, el enemigo dispara y recibe da�o el escudo, RECIBIR DA�O AVANZA INSTANTANEAMENTE AL 10
                //Fortunately, your shield will prevent you from taking damage and it will regenerate after a few seconds
                EventBus.Publish(new OnSetEnemiesCanAttack(false));
                EventBus.Publish(new OnSetShieldsActiveEvent(true));
                EventBus.Publish(new OnSetFirstHeal(false));
                shieldsUi.alpha = 1f;
                break;
            case 11:
                //El enemigo deja de disparar, el escudo se regenera, una vez al maximo AVANZA DIRECTAMENTE AL 11
                //Evento de enemigo deja de disparar
                //Fortunately, your shield will prevent you from taking damage and it will regenerate after a few seconds
                EventBus.Publish(new OnSetEnemiesCanAttack(true));
                EventBus.Publish(new OnSetFirstHeal(true));
                break;
            case 12:
                //Defeated enemies will drop gold and it will be deposited in the gold wagon
                EventBus.Publish(new OnActivateGoldWagon());
                EventBus.Publish(new OnEnableGoldBoxEvent(true));
                EventBus.Publish(new OnSetTutorialEnemyTarget(1));
                break;
            case 13:
                //Aparece la vida del vagon de oro baja, el enemigo dispara de nuevo y lo rompe
                //Evento de enemigo volver a disparar
                EventBus.Publish(new OnSetEnemiesCanAttack(true));
                goldHpUi.alpha = 1f;
                break;
            case 14:
                //You won�t be able to hold gold as long as the wagon is broken, try fixing it.
                //Se descongela al player, aparece la ui de reparar sobre el vagon de oro, se mata al enemigo. Al llegar el oro a la caja se avanza al 13
                //Evento enemigo dejar de disparar
                EventBus.Publish(new OnFreezePlayerEvent(true));
                break;
            case 15:
                EventBus.Publish(new OnSetAttackEnabledEvent(true));
                break;
            case 16:
                //Your gold is still not safe, you must take it to the vault to reclaim it
                //Se congela al player, el texto explica que no esta a salvo
                EventBus.Publish(new OnFreezePlayerEvent(false));
                break;
            case 17:
                //Se descongela al player y aparece la Ui de oro sobre la bolsa, se recoje y se deposita, al depositar SE AVANZA AL 15
                EventBus.Publish(new OnFreezePlayerEvent(true));
                break;
            case 18:
                //Se prende la Ui de oro total
                goldAmountUi.alpha = 1f;
                StartCoroutine(HoldCoroutine(holdDuration));
                break;
            case 19:
                //Texto sobre la tienda
                //You can use the stored gold to buy new wagons and weapons in the stations
                StartCoroutine(HoldCoroutine(holdDuration));
                break;
            case 20:
                //Texto final, Empezar la run
                //Now you�re ready to take on the road!
                PlayerPrefs.SetInt("TutorialCompleted", 1);
                StartCoroutine(HoldCoroutine(holdDuration));
                break;
            case 21:
                Debug.Log("Final");
                EventBus.Publish(new OnStartSpawningEnemiesEvent(true));
                EventBus.Publish(new OnSetCanConsumeEvent(true));
                EventBus.Publish(new OnSetTimerStartedEvent(true));
                EventBus.Publish(new OnSetTutorialVisibleEvent(false));
                break;

            default:
                break;
        }
    }
    private IEnumerator HoldCoroutine(float holdTime)
    {
        yield return new WaitForSeconds(holdTime);
        AdvanceStepNaturally();
    }

    private void AdvanceStepByClicking(OnAdvanceTutorialStepByClick ev)
    {
        if (!playerFrozen) return;

        currentStep += 1;
        if (currentStep >= texts.Length)
        {
            return;
        }
        tutorialText.text = texts[currentStep];
        HandleSteps(currentStep);
    }
    private void AdvanceStepNaturally(OnAdvanceTutorialStep ev)
    {
        currentStep += 1;
        if (currentStep >= texts.Length)
        {
            return;
        }
        tutorialText.text = texts[currentStep];
        HandleSteps(currentStep);
    }
    private void AdvanceStepNaturally()
    {
        currentStep += 1;
        if (currentStep > texts.Length)
        {
            HandleSteps(currentStep);
            return;
        }
        else if (currentStep >= texts.Length)
        {
            tutorialText.text = texts[currentStep -1];
            HandleSteps(currentStep);
            return;
        }
        tutorialText.text = texts[currentStep];
        HandleSteps(currentStep);
    }
}