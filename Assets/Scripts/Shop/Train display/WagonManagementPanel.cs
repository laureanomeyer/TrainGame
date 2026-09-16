using UnityEngine;
using UnityEngine.UI;

public class WagonManagementPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button sellButton;

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

        if(upgradeButton != null)
        {
            upgradeButton.interactable = false;
        }

        Hide();
    }

    private void OnDisable()
    {
        moveButton.onClick.RemoveListener(OnMoveClicked);
        sellButton.onClick.RemoveListener(OnSellClicked);
    }

    public void Show()
    {
        panelRoot.SetActive(true);
        EventBus.Publish(new OnShowCursorEvent(CursorType.Real));
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
}
