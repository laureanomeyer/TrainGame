using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIPlayerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerBrain player;
    [SerializeField] private UIDocument uiDocument;


    [Header("Locomotive UI")]
<<<<<<< Updated upstream
    [SerializeField] private Image fuelFillImage;
    [SerializeField] private Image fuelMaxCapacityImage;
    [SerializeField] private Image shieldImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image consumedImage;
=======
    [SerializeField] private GameObject fuelIndicator;
    [SerializeField] private GameObject fuelMaxCapacityIndicator;
    [SerializeField] private UnityEngine.UI.Image shieldImage;
>>>>>>> Stashed changes

    [Header("Inventory UI")]
    [SerializeField] private UnityEngine.UI.Image coalImage;
    [SerializeField] private UnityEngine.UI.Image goldImage;
    [SerializeField] private TMP_Text goldText;

    [Header("Run Progress")]
    [SerializeField] private SceneRunController sceneRunController;
    [SerializeField] private UnityEngine.UI.Image runProgressFill;
    [SerializeField] private RectTransform trainIcon;
    [SerializeField] private RectTransform startPoint;
    [SerializeField] private RectTransform endPoint;
    [SerializeField] private TMP_Text currentLevel;

    [Header("Interactions")]
    [SerializeField] private GameObject InteractImage;

    private Color originalColor;
<<<<<<< Updated upstream
=======
    private LocomotiveBrain locomotive;
    private VisualElement manecilla;
    private VisualElement hebilla;
>>>>>>> Stashed changes


    private void Start()
    {
        UpdateGoldUI(0);
        InteractImage.SetActive(false);
<<<<<<< Updated upstream
        consumedImage.gameObject.SetActive(false);
=======
        locomotive = RunManager.Instance.LocomotiveBrain;

        var root = uiDocument.rootVisualElement;
        manecilla = root.Q<VisualElement>("Manecilla");
        hebilla = root.Q<VisualElement>("Hebilla");
>>>>>>> Stashed changes
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
        LocomotiveBrain locomotive = RunManager.Instance.LocomotiveBrain;

        if (locomotive != null && locomotive.fuelController != null)
        {
<<<<<<< Updated upstream
            var currentFill = locomotive.fuelController.CurrentFuel / locomotive.fuelController.FuelMaxCapaciy;
            fuelFillImage.fillAmount = currentFill;
            shieldImage.fillAmount = locomotive.fuelController.CurrentShield / locomotive.fuelController.MaxShield;

            var currentCapacity = locomotive.fuelController.CurrentMaxFuel / locomotive.fuelController.FuelMaxCapaciy;
            fuelMaxCapacityImage.fillAmount = currentCapacity;
            consumedImage.fillAmount = currentCapacity;

            if (locomotive.fuelController.CurrentFuel < locomotive.fuelController.FuelMaxCapaciy / 4)
            {
                backgroundImage.color = new Color(1, 0, 0, 0.5f);
                consumedImage.gameObject.SetActive(true);
            }
            else 
            { 
                backgroundImage.color = originalColor;
                consumedImage.gameObject.SetActive(false);
            }

=======
            var currentFuel = locomotive.fuelController.CurrentFuel
                            / locomotive.fuelController.FuelMaxCapaciy;
            var currentCapacity = locomotive.fuelController.CurrentMaxFuel
                                / locomotive.fuelController.FuelMaxCapaciy;

            SetRotation(manecilla, Mathf.Lerp(-90, 80, currentFuel));
            SetRotation(hebilla, Mathf.Lerp(-90, 80, currentCapacity));

            // shieldImage sigue igual por ahora
            shieldImage.fillAmount = locomotive.fuelController.CurrentShield
                                   / locomotive.fuelController.MaxShield;
>>>>>>> Stashed changes
        }
    }

    void SetRotation(VisualElement el, float degrees)
    {
        if (el == null) return;
        el.style.rotate = new StyleRotate(new Angle(degrees, AngleUnit.Degree));
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