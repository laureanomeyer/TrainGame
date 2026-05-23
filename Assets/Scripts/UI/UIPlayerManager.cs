using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPlayerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerBrain player;


    [Header("Locomotive UI")]
    [SerializeField] private GameObject fuelIndicator;
    [SerializeField] private GameObject fuelMaxCapacityIndicator;
    [SerializeField] private Image shieldImage;
    [SerializeField] private GameObject LowFuelImage;

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

    [Header("Interactions")]
    [SerializeField] private GameObject InteractImage;

    private Color originalColor;
    private LocomotiveBrain locomotive;


    private void Start()
    {
        UpdateGoldUI(0);
        InteractImage.SetActive(false);
        locomotive = RunManager.Instance.LocomotiveBrain;
        if (LowFuelImage != null)
            LowFuelImage.SetActive(false);
    }

    void Update()
    {
        //UpdateWagonUI();
        UpdateLocomotiveUI();
        UpdateInventoryUI();
        UpdateRunProgress();
        originalColor = new Color(0, 0, 0, 0.7f);
    }

    void UpdateLocomotiveUI()
    {
        if (locomotive != null && locomotive.fuelController != null)
        {
            var currentFuel = locomotive.fuelController.CurrentFuel / locomotive.fuelController.FuelMaxCapaciy;
            fuelIndicator.transform.rotation = Quaternion.Euler(0,0, Mathf.Lerp(90, -80, currentFuel));

            if (locomotive.fuelController.CurrentFuel < locomotive.fuelController.FuelMaxCapaciy / 3)
            {
                if (LowFuelImage != null)
                    LowFuelImage.SetActive(true);
            }
            else 
            {
                if (LowFuelImage != null)
                    LowFuelImage.SetActive(false);
            } 

            var currentCapacity = locomotive.fuelController.CurrentMaxFuel / locomotive.fuelController.FuelMaxCapaciy;
            fuelMaxCapacityIndicator.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(90, -80, currentCapacity));

            shieldImage.fillAmount = locomotive.fuelController.CurrentShield / locomotive.fuelController.MaxShield;
        }
    }

    void UpdateInventoryUI()
    {
        PlayerInventory inventory = player.Inventory;

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
        GameEvents.OnHideInteract += HideInteract;
        GameEvents.OnShowInteract += ShowInteract;
    }

    private void OnDisable()
    {
        GameEvents.OnGoldBoxChanged -= UpdateGoldUI;
        GameEvents.OnHideInteract -= HideInteract;
        GameEvents.OnShowInteract -= ShowInteract;
    }



    void UpdateGoldUI(float currentGold)
    {
            goldText.text = "Oro actual: " + currentGold; 
    }

    void UpdateRunProgress()
    {
        if (player.Inventory == null) return;

            float progress = sceneRunController.Progress;

            trainIcon.position = Vector3.Lerp(startPoint.position, endPoint.position, progress);

        if (runProgressFill != null)
        {
            runProgressFill.fillAmount = progress;
        }

        if(currentLevel != null) currentLevel.text = new string("Nivel actual: " + GameManager.Instance.Session.SessionConfig.CurrentLevel);

    }
    void ShowInteract()
    {
        InteractImage.SetActive(true);
    }

    void HideInteract()
    {
        InteractImage.SetActive(false);
    }
}