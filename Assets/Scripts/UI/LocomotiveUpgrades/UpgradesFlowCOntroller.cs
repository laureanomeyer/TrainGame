using UnityEngine;
using UnityEngine.UI;

public class UpgradesFlowController : MonoBehaviour
{
    [SerializeField] private GameObject continueAndUpgrade;
    [SerializeField] private GameObject upgradeGameobject;
    [SerializeField] private LocomotivesUpgradesButtonController upgrades;
    [SerializeField] private GameObject sureMessage;

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
            upgrades.ChangeUsedUpgrades();
        }
    }
    public void ShowContinueAndUpgrade()
    {
        HideAll();
        continueAndUpgrade.SetActive(true);
    }
    public void ShowUpgrades()
    {
        HideAll();
        upgradeGameobject.SetActive(true);
    }
    public void ShowSureMessage()
    {
        HideAll();
        sureMessage.SetActive(true);
    }
    public void ContinueJourney()
    {
        if (upgrades.UsedUpgrade)
        {
            //GameManager.Instance.GoToRun();
            StoreManager.Instance.ExitStore();
        } else
        {
            ShowSureMessage();
        }
    }

    public void ContinueAnywaysn()
    {
        //GameManager.Instance.GoToRun();
        StoreManager.Instance.ExitStore();
    }
    void HideAll()
    {
        continueAndUpgrade.SetActive(false);
        upgradeGameobject.SetActive(false);
        sureMessage.SetActive(false);
    }
}
