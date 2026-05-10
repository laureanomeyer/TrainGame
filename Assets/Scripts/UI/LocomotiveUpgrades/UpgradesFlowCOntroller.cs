using UnityEngine;

public class UpgradesFlowController : MonoBehaviour
{
    [SerializeField] private GameObject continueAndUpgrade;
    [SerializeField] private GameObject upgradeGameobject;
    [SerializeField] private LocomotivesUpgradesButtonController upgrades;
    [SerializeField] private GameObject sureMessage;

    private void Start()
    {
        HideAll();
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
            GameManager.Instance.GoToRun();
        } else
        {
            ShowSureMessage();
        }
    }

    public void ContirnueAnyways()
    {
        GameManager.Instance.GoToRun();
    }
    void HideAll()
    {
        continueAndUpgrade.SetActive(false);
        upgradeGameobject.SetActive(false);
        sureMessage.SetActive(false);
    }
}
