using TMPro;
using UnityEngine;

public class LocomotivesUpgradesButtonController : MonoBehaviour
{
    [SerializeField] private LocomotiveUpgradeButton[] buttons;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var button in buttons)
        {
            button.ButtonManager = this;
        }
    }

    public void DeactivateAllButtons()
    {
        foreach (var button in buttons)
        {
            button.DeactivateButton();
        }
    }
}
