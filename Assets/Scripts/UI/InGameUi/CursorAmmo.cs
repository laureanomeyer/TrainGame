using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorAmmo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image cursorImage;
    [SerializeField] private Image cursorImageCenter;
    [SerializeField] private Vector3 offset;

    private RectTransform cursorRect;
    private RectTransform cursorCenterRect;
    private RectTransform ammoRect;
    private Canvas canvas;

    private float reloadDuration;
    private float reloadTimer;
    private bool isReloading;

    [Header("Crosshair Dinamic Size")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float expandedScale = 1.5f;
    [SerializeField] private float shotCooldownDuration = 0.25f;
    [SerializeField] private float scaleSmoothSpeed = 12f;

    private float shotCooldownTimer;
    private bool isShotCooldown;
    private float targetScale;

    private void Awake()
    {
        cursorRect = cursorImage.rectTransform;
        cursorCenterRect = cursorImageCenter.rectTransform;
        ammoRect = ammoText.rectTransform;
        canvas = GetComponent<Canvas>();

        GameEvents.OnAmmoChanged += UpdateText;
        GameEvents.OnReloadStarted += StartReloadFill;
        GameEvents.OnShoot += StartShootCrosshairAnimation;
        GameEvents.OnShowCursor += SetCursorVisibility;
        TutorialEvents.OnSetAttackEnabled += SetCursorVisibility;
    }

    private void Start()
    {
        cursorImage.fillAmount = 1;

        targetScale = normalScale;

        cursorRect.localScale = Vector3.one * normalScale;
        cursorCenterRect.localScale = Vector3.one * normalScale;

    }
    private void LateUpdate()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
        (RectTransform)canvas.transform,
        mousePos,
        null,
        out Vector2 localPoint
        );

        cursorRect.localPosition = localPoint;
        cursorCenterRect.localPosition = localPoint;
        ammoRect.localPosition = localPoint + (Vector2)offset;
        
        UpdateCrosshairScale();
        UpdateShootCrosshairCooldown();

        if (!isReloading) return;

        reloadTimer += Time.deltaTime;

        float cooldownProgress = reloadTimer / reloadDuration;

        cursorImage.fillAmount = cooldownProgress;

        if (reloadTimer >= reloadDuration) CancelReloadFill();
    }

    private void StartShootCrosshairAnimation(float rateOfFire)
    {
        shotCooldownDuration = rateOfFire;

        shotCooldownTimer = 0;
        isShotCooldown = true;

        targetScale = expandedScale;
    }

    private void UpdateShootCrosshairCooldown()
    {
        if (!isShotCooldown) return;

        shotCooldownTimer += Time.deltaTime;

        float progress = shotCooldownTimer / shotCooldownDuration;
        progress = Mathf.Clamp01(progress);

        targetScale = Mathf.Lerp(expandedScale, normalScale, progress);

        if (progress >= 1f)
        {
            isShotCooldown = false;
            targetScale = normalScale;
        }
    }

    private void UpdateCrosshairScale()
    {
        Vector3 desiredScale = Vector3.one * targetScale;

        cursorRect.localScale = Vector3.Lerp(cursorRect.localScale, desiredScale, Time.deltaTime * scaleSmoothSpeed);
        cursorCenterRect.localScale = Vector3.Lerp(cursorCenterRect.localScale, desiredScale, Time.deltaTime * scaleSmoothSpeed);
    }

    void UpdateText(float currentAmmo)
    {
        ammoText.text = $"{currentAmmo}";
    }

    void StartReloadFill(float reloadDuration)
    {
        this.reloadDuration = reloadDuration;
        reloadTimer = 0;
        isReloading = true;
        cursorImage.fillAmount = 0;
    }

    void CancelReloadFill()
    {
        isReloading = false;
        cursorImage.fillAmount = 1f;
    }

    void SetCursorVisibility(bool visible)
    {
        cursorImage.gameObject.SetActive(visible);
        cursorImageCenter.gameObject.SetActive(visible);
        ammoText.gameObject.SetActive(visible);
    }
    private void OnDestroy()
    {
        GameEvents.OnAmmoChanged -= UpdateText;
        GameEvents.OnReloadStarted -= StartReloadFill;
        GameEvents.OnShoot -= StartShootCrosshairAnimation;
        GameEvents.OnShowCursor -= SetCursorVisibility;
        TutorialEvents.OnSetAttackEnabled -= SetCursorVisibility;
    }
}
