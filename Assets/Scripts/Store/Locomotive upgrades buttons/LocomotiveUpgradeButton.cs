using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocomotiveUpgradeButton : MonoBehaviour
{
    private LocomotivesUpgradesButtonController buttonManager;
    public LocomotivesUpgradesButtonController ButtonManager {  get => buttonManager; set => buttonManager = value; }

    private Button button;

    private TrainStats upgrades;

    [Header("Upgrades")]
    [SerializeField] private float maxHp;
    [SerializeField] private float defense;
    [SerializeField] private float goldMultyplier;
    [SerializeField] private float damageMultyplier;
    [SerializeField] private float attackSpeed;

    [SerializeField] private TextMeshProUGUI description;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();

        upgrades = new TrainStats(maxHp, defense, goldMultyplier, damageMultyplier, attackSpeed, 0, 0);
    }

    public void adquireUpgrade()
    {
        adquiere();
        buttonManager.DeactivateAllButtons();
    }

    public virtual void adquiere()
    {
        GameManager.Instance.TrainData.locomotiveStatsMultiplicator += upgrades;
    }

    public void DeactivateButton()
    {
        button.interactable = false;
    }
}
