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

    private void Awake()
    {
        cursorRect = cursorImage.rectTransform;
        cursorCenterRect = cursorImageCenter.rectTransform;
        ammoRect = ammoText.rectTransform;
        canvas = GetComponent<Canvas>();

        GameEvents.OnAmmoChanged += UpdateText;
        GameEvents.OnReloadStarted += StartReloadFill;
        GameEvents.OnShowCursor += SetCursorVisibility;
        TutorialEvents.OnSetAttackEnabled += SetCursorVisibility;
    }
    private void Start()
    {
        cursorImage.fillAmount = 1;
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

        if (!isReloading) return;

        reloadTimer += Time.deltaTime;
        cursorImage.fillAmount = reloadTimer / reloadDuration;

        if (reloadTimer >= reloadDuration) CancelReloadFill();
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
        GameEvents.OnShowCursor -= SetCursorVisibility;
        TutorialEvents.OnSetAttackEnabled -= SetCursorVisibility;
    }
}
