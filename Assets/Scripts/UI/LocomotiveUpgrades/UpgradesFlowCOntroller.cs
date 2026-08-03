using UnityEngine;
using UnityEngine.UI;

public class UpgradesFlowController : MonoBehaviour
{
    [SerializeField] private GameObject continueAndReorder;
    [SerializeField] private GameObject upgradeGameobject;
    [SerializeField] private LocomotivesUpgradesButtonController upgrades;
    [SerializeField] private Button upgradesPanelButton;

    private int currentLevel;

    private void Start()
    {
        HideAll();

        var sessionConfigRef = ServiceLocator.Get<SessionConfig>();
        currentLevel = sessionConfigRef.CurrentLevel;

        if (currentLevel <= 0)
        {
            upgradesPanelButton.interactable = false;
        }

        ShowUpgrades();
    }

    public void ShowContinueAndReorder()
    {
        HideAll();
        continueAndReorder.SetActive(true);
    }

    public void ShowUpgrades()
    {
        upgradeGameobject.SetActive(true);
        EventBus.Publish(new OnShowCursorEvent(CursorType.Real));
        EventBus.Publish(new OnActivateUiEvent(false));
    }

    public void ContinueJourney()
    {
        StoreManager.Instance.ExitStore();
    }

    void HideAll()
    {
        continueAndReorder.SetActive(false);
        upgradeGameobject.SetActive(false);
    }
}
