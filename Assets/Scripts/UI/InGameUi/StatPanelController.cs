using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class StatPanelController : MonoBehaviour
{
    [Header ("Object References")]
    [SerializeField] private GameObject statsPanelCanvas;
    [SerializeField] private InputActionAsset inputActions;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI shieldsText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI bonusGoldText;

    private InputAction toggleAction;

    void Awake()
    {
        toggleAction = inputActions.FindAction("CallUi");
        toggleAction.Enable();
        toggleAction.performed += OnToggleStats;

        GameEvents.OnStatChanged += SetupTexts;

        SetupTexts();
    }

    void OnDestroy()
    {
        toggleAction.performed -= OnToggleStats;
        toggleAction.Disable();
        GameEvents.OnStatChanged -= SetupTexts;
    }

    private void OnToggleStats(InputAction.CallbackContext ctx)
    {
        if(statsPanelCanvas)
            statsPanelCanvas.SetActive(!statsPanelCanvas.activeSelf);
    }

    private void SetupTexts()
    {
       /* hpText.text = GameManager.Instance.Session.StatSystem.GetStat(StatType.MaxHp) * 100 + "%";
        shieldsText.text = GameManager.Instance.Session.StatSystem.GetStat(StatType.Defense) * 100 + "%";
        damageText.text = GameManager.Instance.Session.StatSystem.GetStat(StatType.DamageMultiplier) * 100 + "%";
        attackSpeedText.text = GameManager.Instance.Session.StatSystem.GetStat(StatType.AttackSpeed) * 100 + "%";
        bonusGoldText.text = GameManager.Instance.Session.StatSystem.GetStat(StatType.GoldMultiplier) * 100 + "%";*/

        hpText.text = GameManager.Instance.Session.StatSystem.GetStat(StatType.MaxHp).ToString();
        shieldsText.text = GameManager.Instance.Session.StatSystem.GetStat(StatType.Defense).ToString();
        damageText.text = GameManager.Instance.Session.StatSystem.GetStat(StatType.DamageMultiplier).ToString();
        attackSpeedText.text = GameManager.Instance.Session.StatSystem.GetStat(StatType.AttackSpeed).ToString();
        bonusGoldText.text = GameManager.Instance.Session.StatSystem.GetStat(StatType.GoldMultiplier).ToString();
    }
}
