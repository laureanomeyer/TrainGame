using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorAmmo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image cursorImage;
    [SerializeField] private Vector3 offset;

    private float reloadDuration;
    private float reloadTimer;
    private bool isReloading;

    private void Start()
    {
        GameEvents.OnAmmoChanged += UpdateText;
        GameEvents.OnReloadStarted += StartReloadFill;

        cursorImage.fillAmount = 1;
    }
    private void LateUpdate()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        ammoText.transform.position = mousePos + (Vector2)offset;
        cursorImage.transform.position = mousePos;

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

    private void OnDestroy()
    {
        GameEvents.OnAmmoChanged -= UpdateText;
        GameEvents.OnReloadStarted -= StartReloadFill;
    }
}
