using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LocomotiveUpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
    [SerializeField] private string textDescription;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(GameManager.Instance.Session.TrainData.locomotiveStatsMultiplicator.trainMaxHp);
        button = GetComponent<Button>();

        upgrades = new TrainStats(maxHp, defense, goldMultyplier, damageMultyplier, attackSpeed, 0, 0);

        button.onClick.AddListener(adquireUpgrade);
    }

    public void adquireUpgrade()
    {
        adquiere();
        buttonManager.DeactivateAllButtons();
    }

    public virtual void adquiere()
    {
        GameManager.Instance.Session.TrainData.locomotiveStatsMultiplicator += upgrades;
        Debug.Log(GameManager.Instance.Session.TrainData.locomotiveStatsMultiplicator.trainMaxHp);
    }

    public void DeactivateButton()
    {
        button.interactable = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable == true)
        {
            description.text = textDescription;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        description.text = "";
    }
}
