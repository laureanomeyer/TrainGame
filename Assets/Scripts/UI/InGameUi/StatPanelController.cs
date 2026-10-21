using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image weaponSprite;

    private InputAction toggleAction;
    private StatSystem statsSystemRef;

    void Awake()
    {
        stats.alpha = 0f;
        toggleAction = inputActions.FindAction("CallUi");
        toggleAction.Enable();
        toggleAction.performed += OnToggleStats;
        statsSystemRef = ServiceLocator.Get<StatSystem>();
        EventBus.Subscribe<OnStatChangedEvent>(CallSetUpTextEvent);
        EventBus.Subscribe<OnAmmoChangedEvent>(UpdateAmmoText);
        EventBus.Subscribe<OnWeaponChangedEvent>(SetWeaponImage);

        SetupTexts();
    }

    void OnDestroy()
    {
        toggleAction.performed -= OnToggleStats;
        toggleAction.Disable();
        EventBus.Unsubscribe<OnStatChangedEvent>(CallSetUpTextEvent);
        EventBus.Unsubscribe<OnAmmoChangedEvent>(UpdateAmmoText);
        EventBus.Unsubscribe<OnWeaponChangedEvent>(SetWeaponImage);
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

    void UpdateAmmoText(OnAmmoChangedEvent ammoChagEvent)
    {
        ammoText.text = $"{ammoChagEvent.Ammunition}";
    }

    private void SetupTexts()
    {
        hpText.text = "x " + FormatStat(statsSystemRef.GetStat(StatType.MaxHp));
        shieldsText.text = "x " + FormatStat(statsSystemRef.GetStat(StatType.Defense) + 1);
        damageText.text = "x " + FormatStat(statsSystemRef.GetStat(StatType.DamageMultiplier));
        attackSpeedText.text = "x " + FormatStat(statsSystemRef.GetStat(StatType.AttackSpeed));
    }

    private string FormatStat(float value)
    {
        return (value % 1 == 0) ? value.ToString("F0") : value.ToString("F1");
    }

    public void SetWeaponImage(OnWeaponChangedEvent ev)
    {
        if (weaponSprite != null) 
        weaponSprite.sprite = ev.sprite;
    }
}
