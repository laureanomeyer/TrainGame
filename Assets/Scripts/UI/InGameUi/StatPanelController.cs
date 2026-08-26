using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class StatPanelController : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private CanvasGroup stats;
    [SerializeField] private InputActionAsset inputActions;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI shieldsText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI bonusGoldText;

    private InputAction toggleAction;
    private StatSystem statsSystemRef;

    void Awake()
    {
        toggleAction = inputActions.FindAction("CallUi");
        toggleAction.Enable();
        toggleAction.performed += OnToggleStats;
        statsSystemRef = ServiceLocator.Get<StatSystem>();
        EventBus.Subscribe<OnStatChangedEvent>(CallSetUpTextEvent);

        SetupTexts();
    }

    void OnDestroy()
    {
        toggleAction.performed -= OnToggleStats;
        toggleAction.Disable();
        EventBus.Unsubscribe<OnStatChangedEvent>(CallSetUpTextEvent);
    }

    private void OnToggleStats(InputAction.CallbackContext ctx)
    {
        if (stats)
        {
            if (stats.alpha != 0)
            {
                stats.alpha = 0;
            }
            else
            {
                stats.alpha = 1;
            }
        }
        else Debug.Log("No stats");
    }

    private void CallSetUpTextEvent(OnStatChangedEvent statsChagedEvent)
    {
        SetupTexts();
    }

    private void SetupTexts()
    {
        hpText.text = "x " + FormatStat(statsSystemRef.GetStat(StatType.MaxHp));
        shieldsText.text = "x " + FormatStat(statsSystemRef.GetStat(StatType.Defense) + 1);
        damageText.text = "x " + FormatStat(statsSystemRef.GetStat(StatType.DamageMultiplier));
        attackSpeedText.text = "x " + FormatStat(statsSystemRef.GetStat(StatType.AttackSpeed));
        bonusGoldText.text = "x " + FormatStat(statsSystemRef.GetStat(StatType.GoldMultiplier));
    }
    private string FormatStat(float value)
    {
        return (value % 1 == 0) ? value.ToString("F0") : value.ToString("F1");
    }
}
