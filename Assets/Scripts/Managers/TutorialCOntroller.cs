using TMPro;
using UnityEngine;
using System;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private string[] texts;
    [SerializeField] private int currentStep = 0;

    [SerializeField] private CanvasGroup darkerFilter;
    [SerializeField] private TextMeshProUGUI tutorialText;

    [SerializeField] private CanvasGroup fuelUi;
    [SerializeField] private CanvasGroup shieldsUi;
    [SerializeField] private CanvasGroup goldHpUi;
    [SerializeField] private CanvasGroup goldAmountUi;

    [SerializeField] Transform EnemySpawn;
    [SerializeField] private EnemyData coalEnemy;
    [SerializeField] private EnemyData commonEnemy;

    /// <summary>What the player is allowed to do while a step is active.</summary>
    private enum PlayerMode
    {
        Keep,
        Reading,
        Interact,
        Gameplay,
        Combat
    }

    private sealed class TutorialStep
    {
        public readonly PlayerMode Mode;
        public readonly Action OnEnter;

        public TutorialStep(PlayerMode mode, Action onEnter = null)
        {
            Mode = mode;
            OnEnter = onEnter;
        }
    }

    private TutorialStep[] steps;
    private bool playerFrozen;
    private bool finished;

    private void Awake()
    {
        BuildSteps();

        tutorialText.text = texts[currentStep];

        fuelUi.alpha = 0f;
        shieldsUi.alpha = 0f;
        goldHpUi.alpha = 0f;
        goldAmountUi.alpha = 0f;

        EventBus.Subscribe<OnFreezePlayerEvent>(SetPlayerFrozen);
        EventBus.Subscribe<OnAdvanceTutorialStepByClick>(AdvanceStepByClicking);
        EventBus.Subscribe<OnAdvanceTutorialStep>(AdvanceStepNaturally);
        EventBus.Subscribe<OnSkipTutorial>(SkipTutorial);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnFreezePlayerEvent>(SetPlayerFrozen);
        EventBus.Unsubscribe<OnAdvanceTutorialStepByClick>(AdvanceStepByClicking);
        EventBus.Unsubscribe<OnAdvanceTutorialStep>(AdvanceStepNaturally);
        EventBus.Unsubscribe<OnSkipTutorial>(SkipTutorial);
    }

    private void Start()
    {
        ApplyMode(PlayerMode.Reading);

        EventBus.Publish(new OnSetShieldsActiveEvent(false));
        EventBus.Publish(new OnEnableGoldBoxEvent(false));
        EventBus.Publish(new OnStartSpawningEnemiesEvent(false));
        EventBus.Publish(new OnSetCanConsumeEvent(false));
        EventBus.Publish(new OnSetTimerStartedEvent(false));
        EventBus.Publish(new OnSetTutorialVisibleEvent(true));
    }

    private void BuildSteps()
    {
        steps = new[]
        {
            // 0  The train is your only way through the road 
            new TutorialStep(PlayerMode.Reading),
 
            // 1  This dial shows the remaining fuel */
            new TutorialStep(PlayerMode.Keep, () => fuelUi.alpha = 1f),
 
            // 2  Don't run out of fuel or the machine explodes 
            new TutorialStep(PlayerMode.Keep, () =>
                EventBus.Publish(new OnStartFuelUseEvent())),
 
            // 3  Try replenishing the fuel 
            new TutorialStep(PlayerMode.Keep, () =>
            {
                EventBus.Publish(new OnEnableCoalBoxEvent(true));
                EventBus.Publish(new OnCoalEarnedEvent(1f));
            }),
 
            // 4  Player refuels; advances naturally to 5 
            new TutorialStep(PlayerMode.Interact, () =>
                EventBus.Publish(new OnShowArrowImage(true))),
 
            // 5  Fuel replenished, but you ran out of reserves 
            new TutorialStep(PlayerMode.Reading),
 
            // 6   Some enemies drop coal when killed 
            new TutorialStep(PlayerMode.Reading, () =>
            {
                EventBus.Publish(new OnSpawnEnemyEvent(EnemySpawn.position, coalEnemy));
            }),

            // 7  unfreeze
            new TutorialStep(PlayerMode.Combat, () =>
            {
                EventBus.Publish(new OnShowClickImage(true));
            }),
 
            // 8  However, others will be more hostile 
            new TutorialStep(PlayerMode.Reading, () =>
            {
                EventBus.Publish(new OnSpawnEnemyEvent(EnemySpawn.position, commonEnemy));
                EventBus.Publish(new OnSetTutorialEnemyTarget(0));
                EventBus.Publish(new OnSetEnemiesFlags(true));
            }),
 
            // 9  Hull damage lowers your max fuel until you reach a station 
            new TutorialStep(PlayerMode.Keep, () =>
                EventBus.Publish(new OnSetEnemiesFlags(true))),

            // Can only be fixed when reaching a station
            new TutorialStep(PlayerMode.Keep),
 
            // 10 Your shield prevents damage and regenerates 
            new TutorialStep(PlayerMode.Keep, () =>
            {
                EventBus.Publish(new OnSetEnemiesFlags(false));
                EventBus.Publish(new OnSetShieldsActiveEvent(true));
                EventBus.Publish(new OnSetFirstHeal(false));
                shieldsUi.alpha = 1f;
            }),
 
            // 11 Enemy stops shooting, shield regenerates 
            new TutorialStep(PlayerMode.Keep, () =>
            {
                EventBus.Publish(new OnSetEnemiesFlags(true));
                EventBus.Publish(new OnSetFirstHeal(true));
            }),
 
            // 12 Defeated enemies drop gold, deposited in the gold wagon 
            new TutorialStep(PlayerMode.Keep, () =>
            {
                EventBus.Publish(new OnActivateGoldWagon());
                EventBus.Publish(new OnEnableGoldBoxEvent(true));
                EventBus.Publish(new OnSetTutorialEnemyTarget(1));
                goldHpUi.alpha = 1f;
            }),
 
            // 13 Enemy shoots again and breaks the gold wagon 
            new TutorialStep(PlayerMode.Keep, () =>
            {
                EventBus.Publish(new OnSetEnemiesFlags(true));
                goldHpUi.alpha = 1f;
            }),
 
            // 14 Can't hold gold while the wagon is broken: fix it 
            new TutorialStep(PlayerMode.Interact, () =>
                EventBus.Publish(new OnSetEnemiesFlags(false))),
 
            // 15 (free play: kill the enemy, gold reaches the box) 
            new TutorialStep(PlayerMode.Combat),
 
            // 16 Your gold is not safe yet: take it to the vault 
            new TutorialStep(PlayerMode.Reading),
 
            // 17 Pick up the gold bag and deposit it; advances on deposit 
            new TutorialStep(PlayerMode.Gameplay),
 
            // 18 Total gold UI appears */
            new TutorialStep(PlayerMode.Reading, () => goldAmountUi.alpha = 1f),
 
            // 19 Use stored gold to buy wagons and weapons in stations 
            new TutorialStep(PlayerMode.Reading),
 
            // 20 Now you're ready to take on the road! 
            new TutorialStep(PlayerMode.Keep, FinishTutorial),
        };
    }

    private void ApplyMode(PlayerMode mode)
    {
        switch (mode)
        {
            case PlayerMode.Reading: SetMode(false, false, CursorType.HiddenAndFrozen); break;
            case PlayerMode.Interact: SetMode(true, false, CursorType.HiddenAndMoveable); break;
            case PlayerMode.Gameplay: SetMode(true, false, CursorType.Gameplay); break;
            case PlayerMode.Combat: SetMode(true, true, CursorType.Gameplay); break;
            case PlayerMode.Keep:
            default: break;
        }
    }

    private void SetMode(bool playerActive, bool attackEnabled, CursorType cursor)
    {
        EventBus.Publish(new OnFreezePlayerEvent(playerActive));
        EventBus.Publish(new OnSetAttackEnabledEvent(attackEnabled));
        EventBus.Publish(new OnShowCursorEvent(cursor));
    }

    private void SetPlayerFrozen(OnFreezePlayerEvent ev)
    {
        playerFrozen = !ev.Activated;

        darkerFilter.alpha = playerFrozen ? 1f : 0f;
        EventBus.Publish(new OnSetTutorialVisibleEvent(playerFrozen));
    }

    private void AdvanceStepByClicking(OnAdvanceTutorialStepByClick ev)
    {
        if (!playerFrozen) return;
        Advance();
    }

    private void AdvanceStepNaturally(OnAdvanceTutorialStep ev) => Advance();

    private void SkipTutorial(OnSkipTutorial ev)
    {
        if (PauseMenuManager.Instance.IsPaused) return;
        FinishTutorial();
    }

    private void Advance()
    {
        if (finished) return;
        if (PauseMenuManager.Instance.IsPaused) return;

        EnterStep(currentStep + 1);
    }

    private void EnterStep(int index)
    {
        if (index >= steps.Length)
        {
            FinishTutorial();
            return;
        }

        currentStep = index;

        if (index < texts.Length) tutorialText.text = texts[index];

        ApplyMode(steps[index].Mode);
        steps[index].OnEnter?.Invoke();
    }

    private void FinishTutorial()
    {
        if (finished) return;
        finished = true;

        EventBus.Publish(new OnFreezePlayerEvent(true));
        EventBus.Publish(new OnStartSpawningEnemiesEvent(true));
        EventBus.Publish(new OnEnableGoldBoxEvent(true));
        EventBus.Publish(new OnSetCanConsumeEvent(true));
        EventBus.Publish(new OnSetTimerStartedEvent(true));
        EventBus.Publish(new OnSetTutorialVisibleEvent(false));
        EventBus.Publish(new OnSetAttackEnabledEvent(true));
        EventBus.Publish(new OnSetShieldsActiveEvent(true));
        EventBus.Publish(new OnEnableCoalBoxEvent(true));

        fuelUi.alpha = 1f;
        shieldsUi.alpha = 1f;
        goldHpUi.alpha = 1f;
        goldAmountUi.alpha = 1f;

        PlayerPrefs.SetInt("TutorialCompleted", 1);
    }

}