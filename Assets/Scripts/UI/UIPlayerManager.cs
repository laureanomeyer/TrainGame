using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPlayerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteractions playerInteractions;

    [Header("Wagon UI")]
    [SerializeField] private Image wagonHpImage;
    [SerializeField] private Image wagonHpBackground;


    [Header("Locomotive UI")]
    [SerializeField] private Image fuelFillImage;
    [SerializeField] private Image fuelMaxCapacityImage;
    [SerializeField] private Image shieldImage;

    [Header("Inventory UI")]
    [SerializeField] private Image CoalImage;
    [SerializeField] private TMP_Text goldText;

    [Header("Run Progress")]
    [SerializeField] private SceneRunController sceneRunController;
    [SerializeField] private Image runProgressFill;
    [SerializeField] private RectTransform trainIcon;
    [SerializeField] private RectTransform startPoint;
    [SerializeField] private RectTransform endPoint;


    private void Start()
    {
        UpdateGoldUI(0);
    }

    void Update()
    {
        UpdateWagonUI();
        UpdateLocomotiveUI();
        UpdateCoalUI();
        UpdateRunProgress();
    }

    void UpdateWagonUI()
    {
        WagonBrain wagon = playerInteractions.CurrentWagon;

        if (wagon != null)
        {
            wagonHpImage.gameObject.SetActive(true);
            wagonHpBackground.gameObject.SetActive(true);
            wagonHpImage.fillAmount = wagon.HPController.CurrentHp / wagon.HPController.MaxHp;
        }
        else
        {
            wagonHpImage.gameObject.SetActive(false);
            wagonHpBackground.gameObject.SetActive(false);
        }
    }

    void UpdateLocomotiveUI()
    {
        LocomotiveBrain locomotive = RunManager.Instance.LocomotiveBrain;

        if (locomotive != null && locomotive.fuelController != null)
        {
            fuelFillImage.fillAmount = locomotive.fuelController.CurrentFuel / locomotive.fuelController.FuelMaxCapaciy;
            fuelMaxCapacityImage.fillAmount = locomotive.fuelController.CurrentMaxFuel / locomotive.fuelController.FuelMaxCapaciy;
            shieldImage.fillAmount = locomotive.fuelController.CurrentShield / locomotive.fuelController.MaxShield;
        }
    }

    void UpdateCoalUI()
    {
        PlayerInventory inventory = playerInteractions.Inventory;

        if (inventory != null)
        {
            if (inventory.HasCoal)
            {
                CoalImage.fillAmount = 1;
            }
            else
            {
                CoalImage.fillAmount = 0;
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

    }
}