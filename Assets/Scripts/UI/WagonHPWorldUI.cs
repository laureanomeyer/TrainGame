using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WagonHPWorldUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WagonBrain wagon;

    private PlayerInteractions playerInteractions;

    [Header("UI")]
    [SerializeField] private Image wagonHpImage;
    [SerializeField] private Image wagonHpBackground;
    [SerializeField] private TMP_Text wagonHpText;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;

        if (wagon == null)
        {
            wagon = GetComponentInParent<WagonBrain>();
        }

        playerInteractions = FindFirstObjectByType<PlayerInteractions>();

        SetVisible(false);
    }

    private void Update()
    {
        if (wagon == null || playerInteractions == null || wagon.HPController == null)
        {
            SetVisible(false);
            return;
        }

        bool isCurrentWagon = playerInteractions.CurrentWagon == wagon;

        if (!isCurrentWagon)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);
        UpdateHpBar();
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        transform.forward = cam.transform.forward;
    }

    private void UpdateHpBar()
    {
        float currentHp = wagon.HPController.CurrentHp;
        float maxHp = wagon.HPController.MaxHp;

        wagonHpImage.fillAmount = currentHp / maxHp;

        if (wagonHpText != null)
        {
            wagonHpText.text = $"Wagon HP {currentHp:0}/{maxHp:0}";
        }
    }

    private void SetVisible(bool visible)
    {
        if (wagonHpImage != null)
        {
            wagonHpImage.gameObject.SetActive(visible);
        }

        if (wagonHpBackground != null)
        {
            wagonHpBackground.gameObject.SetActive(visible);
        }

        if (wagonHpText != null)
        {
            wagonHpText.gameObject.SetActive(visible);
        }
    }
}
