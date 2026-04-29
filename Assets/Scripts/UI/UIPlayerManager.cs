using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPlayerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteractions playerInteractions;


    [Header("Locomotive UI")]
    [SerializeField] private Image fuelFillImage;
    [SerializeField] private Image fuelMaxCapacityImage;
    [SerializeField] private Image shieldImage;
    [SerializeField] private Image backgroundImage;

    [Header("Inventory UI")]
    [SerializeField] private Image coalImage;
    [SerializeField] private Image goldImage;
    [SerializeField] private TMP_Text goldText;

    [Header("Run Progress")]
    [SerializeField] private SceneRunController sceneRunController;
    [SerializeField] private Image runProgressFill;
    [SerializeField] private RectTransform trainIcon;
    [SerializeField] private RectTransform startPoint;
    [SerializeField] private RectTransform endPoint;
    [SerializeField] private TMP_Text currentLevel;

    private Color originalColor;


    private void Start()
    {
        UpdateGoldUI(0);
    }

    void Update()
    {
        //UpdateWagonUI();
        UpdateLocomotiveUI();
        UpdateInventoryUI();
        UpdateRunProgress();
        originalColor = new Color(0, 0.39215f, 0, 0.7f);
    }

    void UpdateLocomotiveUI()
    {
        LocomotiveBrain locomotive = RunManager.Instance.LocomotiveBrain;

        if (locomotive != null && locomotive.fuelController != null)
        {
            var currentFill = locomotive.fuelController.CurrentFuel / locomotive.fuelController.FuelMaxCapaciy;
            fuelFillImage.fillAmount = currentFill;
            shieldImage.fillAmount = locomotive.fuelController.CurrentShield / locomotive.fuelController.MaxShield;

            var currentCapacity = locomotive.fuelController.CurrentMaxFuel / locomotive.fuelController.FuelMaxCapaciy;
            fuelMaxCapacityImage.fillAmount = currentCapacity;

            if (locomotive.fuelController.CurrentFuel < locomotive.fuelController.FuelMaxCapaciy / 4)
            {
                backgroundImage.color = new Color(1, 0, 0, 0.5f);
            }
            else backgroundImage.color = originalColor;
        }
    }

    void UpdateInventoryUI()
    {
        PlayerInventory inventory = playerInteractions.Inventory;

        if (inventory != null)
        {
            if (inventory.HasCoal)
            {
                coalImage.fillAmount = 1;
            }
            else
            {
                coalImage.fillAmount = 0;
            }
            if (inventory.GoldAmount > 0)
            {
                goldImage.fillAmount = 1;
            }
            else
            {
                goldImage.fillAmount = 0;
            }
        }
    }

    private void OnEnable()
    {
        GameEvents.OnGoldBoxChanged += UpdateGoldUI;
    }

    private void OnDisable()
    {
        GameEvents.OnGoldBoxChanged -= UpdateGoldUI;
    }



    void UpdateGoldUI(float currentGold)
    {
            goldText.text = "Gold: " + currentGold; 
    }

    void UpdateRunProgress()
    {
        if (playerInteractions.Inventory == null) return;

            float progress = sceneRunController.Progress;

            trainIcon.position = Vector3.Lerp(startPoint.position, endPoint.position, progress);

        if (runProgressFill != null)
        {
            runProgressFill.fillAmount = progress;
        }

        currentLevel.text = new string("Current Level: " + GameManager.Instance.RunNumber);

    }
}