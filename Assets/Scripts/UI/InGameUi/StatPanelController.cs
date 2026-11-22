using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StatPanelController : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private CanvasGroup stats;
    [SerializeField] private RectTransform panelTransform;
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

    private bool isOnRight;    
    private bool targetRight;  
    private float canvasWidth;

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

        EventBus.Subscribe<OnCallActiveStatsPanelUI>(CallShowPanel);
        EventBus.Subscribe<OnCallDeactiveStatsPanelUI>(CallHidePanel);
        EventBus.Subscribe<OnSwitchStatsSide>(CallSwitchSide);
        EventBus.Subscribe<OnResetStatsSide>(CallResetSide);

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

        EventBus.Unsubscribe<OnCallActiveStatsPanelUI>(CallShowPanel);
        EventBus.Unsubscribe<OnCallDeactiveStatsPanelUI>(CallHidePanel);
        EventBus.Unsubscribe<OnSwitchStatsSide>(CallSwitchSide);
        EventBus.Unsubscribe<OnResetStatsSide>(CallResetSide);
    }

    private void SetupSlide()
    {
        statsRect = stats.GetComponent<RectTransform>();

        shownPosition = statsRect.anchoredPosition;

        RectTransform canvasRect = stats.GetComponentInParent<Canvas>().rootCanvas.GetComponent<RectTransform>();
        canvasWidth = canvasRect.rect.width;

        UpdateHiddenPosition();

        statsRect.anchoredPosition = hiddenPosition;
        stats.alpha = 0f;
        isVisible = false;
    }

    private void UpdateHiddenPosition()
    {
        Vector2 dir = isOnRight ? Vector2.right : Vector2.left;
        hiddenPosition = shownPosition + dir * canvasWidth;
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

    private void CallShowPanel(OnCallActiveStatsPanelUI gameEvent)
    {
        ShowPanel();
    }

    private void CallHidePanel(OnCallDeactiveStatsPanelUI gameEvent)
    {
        HidePanel();
    }

    private void CallSwitchSide(OnSwitchStatsSide ev)
    {
        SetTargetSide(true);
    }

    private void CallResetSide(OnResetStatsSide ev)
    {
        SetTargetSide(false);
    }

    private void SetTargetSide(bool right)
    {
        targetRight = right;

        if (targetRight == isOnRight)
        {
            if (isVisible) ShowPanel();
            return;
        }

        slideTween?.Kill();

        if (!isVisible)
        {
            ApplyPendingSide();
            return;
        }

        slideTween = statsRect
            .DOAnchorPos(hiddenPosition, slideDuration)
            .SetEase(hideEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                ApplyPendingSide();
                ShowPanel();
            });
    }

    private void ApplyPendingSide()
    {
        if (targetRight == isOnRight) return;
        isOnRight = targetRight;

        if (panelTransform != null)
            MirrorRect(panelTransform);

        statsRect.anchoredPosition = shownPosition;
        MirrorRect(statsRect);
        shownPosition = statsRect.anchoredPosition;
        UpdateHiddenPosition();
        statsRect.anchoredPosition = hiddenPosition;
    }

    private static void MirrorRect(RectTransform r)
    {
        Vector2 min = r.anchorMin, max = r.anchorMax;
        Vector2 omin = r.offsetMin, omax = r.offsetMax;

        r.anchorMin = new Vector2(1f - max.x, min.y);
        r.anchorMax = new Vector2(1f - min.x, max.y);
        r.offsetMin = new Vector2(-omax.x, omin.y);
        r.offsetMax = new Vector2(-omin.x, omax.y);
        r.pivot = new Vector2(1f - r.pivot.x, r.pivot.y);
    }

    private void ShowPanel()
    {
        if (!isVisible) ApplyPendingSide();   
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
            .OnComplete(() =>
            {
                stats.alpha = 0f;
                ApplyPendingSide();   
            });
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