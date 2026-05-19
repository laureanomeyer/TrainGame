using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocomotivesUpgradesButtonController : MonoBehaviour
{
    #region Buttons
    
    [Header("Max HP Button")]
    [SerializeField] private LocomotiveUpgradeButton maxHPButton;

    [Header("Attack speed Button")]
    [SerializeField] private LocomotiveUpgradeButton attackSpeedButton;

    [Header("Damage multiplier Button")]
    [SerializeField] private LocomotiveUpgradeButton damageMultiplierButton;

    [Header("Max defense Button")]
    [SerializeField] private LocomotiveUpgradeButton defenseButton;

    [Header("Gold multiplier Button")]
    [SerializeField] private LocomotiveUpgradeButton goldMultiplierButton;
    
    #endregion

    [Header("Self")]
    [SerializeField] private GameObject self;

    [Header("Level Upgrades")]
    [SerializeField] private LevelLocomotivesUpgradesSO[] upgradesLevel;
    private Dictionary <int, LevelLocomotivesUpgradesSO> upgradesLevelDictionary = new Dictionary <int, LevelLocomotivesUpgradesSO>();

    private LevelLocomotivesUpgradesSO currentLevelUpgrades;

    private int currentLevel;
    //[SerializeField] private int maxLevel;

    public TrainStats locomotiveUpgrade;

    [Header("UI components")]
    [SerializeField] public TextMeshProUGUI descriptionUI;

    private bool usedUpgrades;
    public bool UsedUpgrade => usedUpgrades;

    void Start()
    {
        self = this.gameObject;

        currentLevel = GameManager.Instance.Session.SessionConfig.CurrentLevel;

        if (upgradesLevel.Length > 0)
        {
            foreach (var upgrade in upgradesLevel)
            {
                upgradesLevelDictionary.Add(upgrade.level, upgrade);
            }
        }

        if (currentLevel > upgradesLevelDictionary.Count)
        {
            currentLevelUpgrades = upgradesLevelDictionary[upgradesLevelDictionary.Count];
        }
        else
        {
            if (upgradesLevelDictionary.ContainsKey(currentLevel))
            {
                currentLevelUpgrades = upgradesLevelDictionary[currentLevel];
            }
            else
            {
                currentLevelUpgrades = null;
            }
        }

        locomotiveUpgrade = new TrainStats();
        usedUpgrades = false;

        maxHPButton.ButtonManager = this;
        attackSpeedButton.ButtonManager = this;
        damageMultiplierButton.ButtonManager = this;
        defenseButton.ButtonManager = this;
        goldMultiplierButton.ButtonManager = this;

        if (currentLevelUpgrades)
        {
            maxHPButton.button.onClick.AddListener(MaxHPUpgrade);
            attackSpeedButton.button.onClick.AddListener(AttackSpeedUpgrade);
            damageMultiplierButton.button.onClick.AddListener(DamageMultiplierUpgrade);
            defenseButton.button.onClick.AddListener(DefenseUpgrade);
            goldMultiplierButton.button.onClick.AddListener(GoldMultiplierUpgrade);

            maxHPButton.Upgrade = currentLevelUpgrades.maxHp;
            attackSpeedButton.Upgrade = currentLevelUpgrades.attackSpeed;
            damageMultiplierButton.Upgrade = currentLevelUpgrades.damageMultiplier;
            defenseButton.Upgrade = currentLevelUpgrades.defense;
            goldMultiplierButton.Upgrade = currentLevelUpgrades.goldMultiplier;
        }
        else
        {
            DeactivateAllButtons();
        }
        
    }

    public void MaxHPUpgrade()
    {
        locomotiveUpgrade.trainMaxHp = currentLevelUpgrades.maxHp;
        GameManager.Instance.Session.TrainData.locomotiveStatsMultiplicator += locomotiveUpgrade;
        DeactivateAllButtons();
    }
    public void AttackSpeedUpgrade()
    {
        locomotiveUpgrade.attackSpeed = currentLevelUpgrades.attackSpeed;
        GameManager.Instance.Session.TrainData.locomotiveStatsMultiplicator += locomotiveUpgrade;
        DeactivateAllButtons();
    }
    public void DamageMultiplierUpgrade()
    {
        locomotiveUpgrade.damageBonus = currentLevelUpgrades.damageMultiplier;
        GameManager.Instance.Session.TrainData.locomotiveStatsMultiplicator += locomotiveUpgrade;
        DeactivateAllButtons();
    }
    public void DefenseUpgrade()
    {
        locomotiveUpgrade.shields = currentLevelUpgrades.defense;
        GameManager.Instance.Session.TrainData.locomotiveStatsMultiplicator += locomotiveUpgrade;
        DeactivateAllButtons();
    }
    public void GoldMultiplierUpgrade()
    {
        locomotiveUpgrade.goldBonus = currentLevelUpgrades.goldMultiplier;
        GameManager.Instance.Session.TrainData.locomotiveStatsMultiplicator += locomotiveUpgrade;
        DeactivateAllButtons();
    }

    public void DeactivateAllButtons()
    {
        maxHPButton.DeactivateButton();
        attackSpeedButton.DeactivateButton();
        damageMultiplierButton.DeactivateButton();
        defenseButton.DeactivateButton();
        goldMultiplierButton.DeactivateButton();

        usedUpgrades = true;

        DeactivateUpgradesUi();
    }
    public void DeactivateUpgradesUi()
    {
        self.SetActive(false);
    }
    public void ActivateUpgradesUi()
    {
        self.SetActive(true);
    }

    public void ChangeUsedUpgrades()
    {
        usedUpgrades = true;
    }
}
