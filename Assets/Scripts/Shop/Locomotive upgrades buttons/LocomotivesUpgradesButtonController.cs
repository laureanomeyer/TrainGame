using System;
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


    [Header("Level Upgrades")]
    [SerializeField] private LevelLocomotivesUpgradesSO[] upgradesLevel;
    private Dictionary <int, LevelLocomotivesUpgradesSO> upgradesLevelDictionary;

    private LevelLocomotivesUpgradesSO currentLevelUpgrades;
    
    private GameObject self;

    private int currentLevel = 1;

    public TrainStats locomotiveUpgrade;
    private TrainData trainDataRef;

    [Header("UI components")]
    [SerializeField] public TextMeshProUGUI descriptionUI;

    void Start()
    {
        trainDataRef = ServiceLocator.Get<TrainData>();

        upgradesLevelDictionary = new Dictionary<int, LevelLocomotivesUpgradesSO>();
        self = this.gameObject;

        if (upgradesLevel.Length > 0)
        {
            foreach (var upgrade in upgradesLevel)
            {
                upgradesLevelDictionary.Add(upgrade.level, upgrade);
            }
        }
        
        currentLevelUpgrades = upgradesLevelDictionary[currentLevel];

        locomotiveUpgrade = new TrainStats();

        maxHPButton.ButtonManager = this;
        attackSpeedButton.ButtonManager = this;
        //Line 67
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
        }
        else
        {
            DeactivateAllButtons();
        }

        EventBus.Publish(new OnActivateNonPausableUI(false));
        EventBus.Publish(new OnActivateUiEvent(false));
    }

    public void MaxHPUpgrade()
    {
        locomotiveUpgrade.trainMaxHp = GetCurrentUpgrade(StatType.MaxHp).maxHp;
        trainDataRef.AddStats(locomotiveUpgrade);
        DeactivateAllButtons();

        GameManager.Instance.Session.RebuildStatsSystem();
    }
    public void AttackSpeedUpgrade()
    {
        locomotiveUpgrade.attackSpeed = GetCurrentUpgrade(StatType.AttackSpeed).attackSpeed;
        trainDataRef.AddStats(locomotiveUpgrade);
        DeactivateAllButtons();

        GameManager.Instance.Session.RebuildStatsSystem();
    }
    public void DamageMultiplierUpgrade()
    {
        locomotiveUpgrade.damageBonus = GetCurrentUpgrade(StatType.DamageMultiplier).damageMultiplier;
        trainDataRef.AddStats(locomotiveUpgrade);
        DeactivateAllButtons();

        GameManager.Instance.Session.RebuildStatsSystem();
    }
    public void DefenseUpgrade()
    {
        locomotiveUpgrade.shields = GetCurrentUpgrade(StatType.Defense).defense;
        trainDataRef.AddStats(locomotiveUpgrade);
        DeactivateAllButtons();

        GameManager.Instance.Session.RebuildStatsSystem();
    }
    public void GoldMultiplierUpgrade()
    {
        locomotiveUpgrade.goldBonus = GetCurrentUpgrade(StatType.GoldMultiplier).goldMultiplier;
        trainDataRef.AddStats(locomotiveUpgrade);
        DeactivateAllButtons();

        GameManager.Instance.Session.RebuildStatsSystem();
    }

    private LevelLocomotivesUpgradesSO GetCurrentUpgrade(StatType type)
    {
        int levelT = trainDataRef.GetStatLevel(type);
        var level = upgradesLevelDictionary[levelT];
        return level;
    }
    public void DeactivateAllButtons()
    {
        maxHPButton.DeactivateButton();
        attackSpeedButton.DeactivateButton();
        damageMultiplierButton.DeactivateButton();
        defenseButton.DeactivateButton();
        goldMultiplierButton.DeactivateButton();

        DeactivateUpgradesUi();

        EventBus.Publish(new OnShowCursorEvent(CursorType.Gameplay));
        EventBus.Publish(new OnActivateUiEvent(true));
        EventBus.Publish(new OnActivateNonPausableUI(true));
        GameManager.Instance.ChangeGameState(GameState.Gameplay);
    }
    public void DeactivateUpgradesUi()
    {
        self.SetActive(false);
    }
}
