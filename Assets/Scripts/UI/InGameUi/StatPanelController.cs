using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StatPanelController : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private CanvasGroup stats;
    [SerializeField] private InputActionAsset inputActions;

    [Header("Slide Animation")]
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI shieldsText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image weaponSprite;

    private InputAction toggleAction;
    private StatSystem statsSystemRef;

    private RectTransform statsRect;
    private Vector2 shownPosition;
    private Vector2 hiddenPosition;
    private bool isVisible;
    private Tween slideTween;

    void Awake()
    {
        SetupSlide();

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
        slideTween?.Kill();
        toggleAction.performed -= OnToggleStats;
        toggleAction.Disable();
        EventBus.Unsubscribe<OnStatChangedEvent>(CallSetUpTextEvent);
        EventBus.Unsubscribe<OnAmmoChangedEvent>(UpdateAmmoText);
        EventBus.Unsubscribe<OnWeaponChangedEvent>(SetWeaponImage);
    }

    private void SetupSlide()
    {
        statsRect = stats.GetComponent<RectTransform>();

        shownPosition = statsRect.anchoredPosition;

        RectTransform canvasRect = stats.GetComponentInParent<Canvas>().rootCanvas.GetComponent<RectTransform>();
        hiddenPosition = shownPosition + Vector2.left * canvasRect.rect.width;

        statsRect.anchoredPosition = hiddenPosition;
        stats.alpha = 0f;
        isVisible = false;
    }

    private void OnToggleStats(InputAction.CallbackContext ctx)
    {
        if (!stats)
        {
            Debug.Log("No stats");
            return;
        }

        if (isVisible) HidePanel();
        else ShowPanel();
    }

    private void ShowPanel()
    {
        isVisible = true;
        slideTween?.Kill();

        stats.alpha = 1f;
        slideTween = statsRect.DOAnchorPos(shownPosition, slideDuration).SetEase(showEase).SetUpdate(true);
    }

    private void HidePanel()
    {
        isVisible = false;
        slideTween?.Kill();

        slideTween = statsRect
            .DOAnchorPos(hiddenPosition, slideDuration)
            .SetEase(hideEase)
            .SetUpdate(true)
            .OnComplete(() => stats.alpha = 0f);
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