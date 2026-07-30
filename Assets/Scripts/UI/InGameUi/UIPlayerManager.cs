using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPlayerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerBrain player;

    [Header("Fuel UI")]
    [SerializeField] private GameObject fuelIndicator;
    [SerializeField] private GameObject fuelMaxCapacityIndicator;
    [SerializeField] private Image fuelImage;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject lowFuel;

    [Header("Shields UI")]
    [SerializeField] private Image shieldImage;
    [SerializeField] private GameObject shieldIndicator;

    [Header("Inventory UI")]
    [SerializeField] private Image coalImage;
    [SerializeField] private Image goldImage;
    [SerializeField] private TMP_Text goldText;

    [Header("Run Progress")]
    [SerializeField] private GameObject bellImage;
    [SerializeField] private SceneRunController sceneRunController;
    [SerializeField] private Image runProgressFill;
    [SerializeField] private RectTransform trainIcon;
    [SerializeField] private RectTransform startPoint;
    [SerializeField] private RectTransform endPoint;
    [SerializeField] private TMP_Text currentLevel;

    private LocomotiveBrain locomotive;
    private float lerpedShield = 1;
    private float lerpedFuel = 1f;
    private float lerpedCapacity = 1f;

    private void Start()
    {
        UpdateGoldUI(0);
        locomotive = RunManager.Instance.LocomotiveBrain;
        if (lowFuel != null)
            lowFuel.SetActive(false);
        if (bellImage) bellImage.SetActive(false);
        SetLevelText();
    }
    private void OnEnable()
    {
        EventBus.Subscribe<OnGoldBoxChangedEvent>(UpdateGoldUIEvent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnGoldBoxChangedEvent>(UpdateGoldUIEvent);
    }

    void Update()
    {
        UpdateLocomotiveUI();
        UpdateInventoryUI();
        UpdateRunProgress();
    }

    void UpdateLocomotiveUI()
    {
        if (locomotive != null && locomotive.fuelController != null)
        {
            if (locomotive.fuelController.CurrentFuel < locomotive.fuelController.FuelMaxCapaciy / 3)
            {
                if (animator != null) animator.SetBool("FuelLow", true);
                if (lowFuel != null)
                    lowFuel.SetActive(true);
            }
            else
            {
                if (animator != null) animator.SetBool("FuelLow", false);
                if (lowFuel != null)
                    lowFuel.SetActive(false);
            }

            var currentShield = locomotive.fuelController.CurrentShield / locomotive.fuelController.MaxShield;
            lerpedShield = Mathf.MoveTowards(lerpedShield, currentShield, Time.deltaTime);
            shieldIndicator.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(90, -90, lerpedShield));
            shieldImage.fillAmount = lerpedShield;

            var currentFuel = locomotive.fuelController.CurrentFuel / locomotive.fuelController.FuelMaxCapaciy;
            lerpedFuel = Mathf.MoveTowards(lerpedFuel, currentFuel, Time.deltaTime);
            fuelIndicator.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(90, -80, lerpedFuel));

            var currentCapacity = locomotive.fuelController.CurrentMaxFuel / locomotive.fuelController.FuelMaxCapaciy;
            lerpedCapacity = Mathf.MoveTowards(lerpedCapacity, currentCapacity, Time.deltaTime);
            fuelImage.fillAmount = lerpedCapacity;
            fuelMaxCapacityIndicator.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(90, -90, lerpedCapacity));
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

    void UpdateGoldUIEvent(OnGoldBoxChangedEvent goldChangedEvent) 
    {
        UpdateGoldUI(goldChangedEvent.CurrentGold);
    }

    void UpdateGoldUI(float currentGold)
    {
        goldText.text = currentGold.ToString();
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

        bellImage.SetActive(progress >= 0.75f);
    }

    void SetLevelText()
    {
        var sessionConfigRef = ServiceLocator.Get<SessionConfig>();
        if (currentLevel != null) currentLevel.text = "Trail " + sessionConfigRef.CurrentLevel;
    }
}