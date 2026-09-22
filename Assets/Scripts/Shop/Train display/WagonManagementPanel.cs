using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Panel de las 3 opciones (Move / Upgrade / Sell) que aparece al seleccionar
// un wagon en modo reorder.
public class WagonManagementPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button sellButton;

    [Header("Opcional")]
    [SerializeField] private TMP_Text upgradeCostText;

    private ReorderManager reorderManagerRef;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void OnEnable()
    {
        ServiceLocator.TryGet(out reorderManagerRef);

        moveButton.onClick.AddListener(OnMoveClicked);
        sellButton.onClick.AddListener(OnSellClicked);
        upgradeButton.onClick.AddListener(OnUpgradeClicked);

        Hide();
    }

    private void OnDisable()
    {
        moveButton.onClick.RemoveListener(OnMoveClicked);
        sellButton.onClick.RemoveListener(OnSellClicked);
        upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
    }

    // upgradeCost == null => este wagon no puede mejorarse (sin LevelSet o nivel máximo)
    public void Show(float? upgradeCost)
    {
        panelRoot.SetActive(true);
        EventBus.Publish(new OnShowCursorEvent(CursorType.Real));

        bool canUpgrade = upgradeCost.HasValue;
        upgradeButton.interactable = canUpgrade;

        if (upgradeCostText != null)
            upgradeCostText.text = canUpgrade ? $"-${upgradeCost.Value:0}" : "";
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
        EventBus.Publish(new OnShowCursorEvent(CursorType.Hidden));
    }

    private void OnMoveClicked()
    {
        if (reorderManagerRef == null) ServiceLocator.TryGet(out reorderManagerRef);
        reorderManagerRef?.ConfirmMoveSelected();
    }

    private void OnSellClicked()
    {
        if (reorderManagerRef == null) ServiceLocator.TryGet(out reorderManagerRef);
        reorderManagerRef?.ConfirmSellSelected();
    }

    private void OnUpgradeClicked()
    {
        if (reorderManagerRef == null) ServiceLocator.TryGet(out reorderManagerRef);
        reorderManagerRef?.ConfirmUpgradeSelected();
    }
}