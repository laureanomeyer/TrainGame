using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WagonManagementPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private TextMeshProUGUI upgradeLabel; // opcional

    private ReorderManager reorderManagerRef;

    private void OnEnable()
    {
        ServiceLocator.Register(this);
        ServiceLocator.TryGet(out reorderManagerRef);

        moveButton.onClick.AddListener(OnMoveClicked);
        sellButton.onClick.AddListener(OnSellClicked);
        if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);

        // Estado inicial: oculto, sin tocar el cursor (eso lo decide quien abre/cierra el panel)
        panelRoot.SetActive(false);
    }

    private void OnDisable()
    {
        moveButton.onClick.RemoveListener(OnMoveClicked);
        sellButton.onClick.RemoveListener(OnSellClicked);
        if (upgradeButton != null) upgradeButton.onClick.RemoveListener(OnUpgradeClicked);

        ServiceLocator.Unregister<WagonManagementPanel>();
    }

    public void Show()
    {
        panelRoot.SetActive(true);
        EventBus.Publish(new OnShowCursorEvent(CursorType.Real));
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
        EventBus.Publish(new OnShowCursorEvent(CursorType.HiddenAndFrozen));
    }

    public void SetUpgradeState(bool hasUpgrade, float cost, bool canAfford)
    {
        if (upgradeButton != null)
            upgradeButton.interactable = hasUpgrade && canAfford;

        if (upgradeLabel != null)
            upgradeLabel.text = hasUpgrade ? $"Mejorar ${cost}" : "Nivel máximo";
    }

    private ReorderManager Manager
    {
        get
        {
            if (reorderManagerRef == null) ServiceLocator.TryGet(out reorderManagerRef);
            return reorderManagerRef;
        }
    }

    private void OnMoveClicked() => Manager?.ConfirmMoveSelected();
    private void OnSellClicked() => Manager?.ConfirmSellSelected();
    private void OnUpgradeClicked() => Manager?.ConfirmUpgradeSelected();
}