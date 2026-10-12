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

    [Header("GoldWagon UI")]
    [SerializeField] private Image goldenWagonHp;

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

    private WagonHP goldenWagonHpController;
    private LocomotiveFuel fuel;
    private PlayerInventory inventory;

    private float lerpedShield = 1;
    private float lerpedFuel = 1f;
    private float lerpedCapacity = 1f;
    private float lerpedGoldCapacity = 1f;

    private void Start()
    {
        UpdateGoldUI(0);
        fuel = RunManager.Instance.LocomotiveBrain.fuelController;
        if (lowFuel != null)
            lowFuel.SetActive(false);
        if (bellImage) bellImage.SetActive(false);
        SetLevelText();
        inventory = player.Inventory;

        ForceUpdateCapacityUi();
        ForceUpdateShieldUI();
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
      /*  if (fuel != null)
        {
            if (fuel.CurrentFuel < fuel.FuelMaxCapaciy / 3)
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

            var currentShield = fuel.CurrentShield / fuel.MaxShield;
            lerpedShield = Mathf.MoveTowards(lerpedShield, currentShield, Time.deltaTime);
            shieldImage.fillAmount = lerpedShield;

            var currentFuel = fuel.CurrentFuel / fuel.FuelMaxCapaciy;
            lerpedFuel = Mathf.MoveTowards(lerpedFuel, currentFuel, Time.deltaTime);
            fuelIndicator.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(80, -80, lerpedFuel));

            var currentCapacity = fuel.CurrentMaxFuel / fuel.FuelMaxCapaciy;
            lerpedCapacity = Mathf.MoveTowards(lerpedCapacity, currentCapacity, Time.deltaTime);
            fuelImage.fillAmount = lerpedCapacity;
            fuelMaxCapacityIndicator.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(80, -80, lerpedCapacity));

            if (goldenWagonHpController != null)
            {
                var currentGoldenWagonHp = goldenWagonHpController.CurrentHp / goldenWagonHpController.MaxHp;
                lerpedGoldCapacity = Mathf.MoveTowards(lerpedGoldCapacity, currentGoldenWagonHp, Time.deltaTime);

                goldenWagonHp.fillAmount = lerpedGoldCapacity;
            }
            else ServiceLocator.TryGet<WagonHP>(out goldenWagonHpController);
        }*/

        if (fuel != null)
        {
            UpdateFuelUI();
            UpdateShieldUI();
            UpdateCapacityUi();
            UpdateGoldenWagonHpUI();
        }
    }

    void UpdateFuelUI()
    {
        bool isLow = fuel.CurrentFuel < fuel.FuelMaxCapaciy / 3;
        if (animator != null) animator.SetBool("FuelLow", isLow);
        if (lowFuel != null) lowFuel.SetActive(isLow);

        var currentFuel = fuel.CurrentFuel / fuel.FuelMaxCapaciy;
        lerpedFuel = Mathf.MoveTowards(lerpedFuel, currentFuel, Time.deltaTime);

        fuelIndicator.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(80, -80, lerpedFuel));
    }

    void UpdateShieldUI()
    {
        var currentShield = fuel.CurrentShield / fuel.MaxShield;
        lerpedShield = Mathf.MoveTowards(lerpedShield, currentShield, Time.deltaTime);

        if (lerpedShield == currentShield) return;

        shieldImage.fillAmount = lerpedShield;
    }
    void ForceUpdateShieldUI()
    {
        var currentShield = fuel.CurrentShield / fuel.MaxShield;
        shieldImage.fillAmount = currentShield;
    }

    void UpdateCapacityUi()
    {
        var currentCapacity = fuel.CurrentMaxFuel / fuel.FuelMaxCapaciy;
        lerpedCapacity = Mathf.MoveTowards(lerpedCapacity, currentCapacity, Time.deltaTime);

        if (lerpedCapacity == currentCapacity) return;

        fuelImage.fillAmount = lerpedCapacity;
        fuelMaxCapacityIndicator.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(80, -80, lerpedCapacity));
    }
    void ForceUpdateCapacityUi()
    {
        var currentCapacity = fuel.CurrentMaxFuel / fuel.FuelMaxCapaciy;

        fuelImage.fillAmount = currentCapacity;
        fuelMaxCapacityIndicator.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(80, -80, currentCapacity));
    }

    void UpdateGoldenWagonHpUI()
    {
        if (goldenWagonHpController == null)
        {
            ServiceLocator.TryGet<WagonHP>(out goldenWagonHpController);
            return;
        }

        var currentGoldenWagonHp = goldenWagonHpController.CurrentHp / goldenWagonHpController.MaxHp;
        lerpedGoldCapacity = Mathf.MoveTowards(lerpedGoldCapacity, currentGoldenWagonHp, Time.deltaTime);
        goldenWagonHp.fillAmount = lerpedGoldCapacity;
    }

    void UpdateInventoryUI()
    {
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