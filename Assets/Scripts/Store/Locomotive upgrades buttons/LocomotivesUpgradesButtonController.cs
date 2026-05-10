using UnityEngine;

public class LocomotivesUpgradesButtonController : MonoBehaviour
{
    [SerializeField] private LocomotiveUpgradeButton[] buttons;

    [Header("Self")]
    [SerializeField] private GameObject self;

    private bool usedUpgrade;
    public bool UsedUpgrade => usedUpgrade;
    void Start()
    {
        foreach (var button in buttons)
        {
            button.ButtonManager = this;
        }
        usedUpgrade = false;
    }

    public void DeactivateAllButtons()
    {
        foreach (var button in buttons)
        {
            button.DeactivateButton();
        }
        DeactivateUpgradesUi();
        usedUpgrade = true;
    }
    public void DeactivateUpgradesUi()
    {
        self.SetActive(false);
    }
    public void ActivateUpgradesUi()
    {
        self.SetActive(true);
    }
}
