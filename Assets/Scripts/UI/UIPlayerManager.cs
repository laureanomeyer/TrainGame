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
    }

 //   void UpdateWagonUI()
 //   {
 //       WagonBrain wagon = playerInteractions.CurrentWagon;
 //
 //       if (wagon != null)
 //       {
 //           wagonHpImage.gameObject.SetActive(true);
 //           WagonHpText.gameObject.SetActive(true);
 //           
 //       }
 //       else
 //       {
 //           wagonHpImage.gameObject.SetActive(false);
 //           wagonHpBackground.gameObject.SetActive(false);
 //           WagonHpText.gameObject.SetActive(false);
 //       }
 //   }

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

    }
}