using UnityEngine;
using UnityEngine.UI;

public class UIPlayerManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private PlayerInteractions playerInteractions;

    [Header("Wagon UI")]
    [SerializeField] private Image wagonHpImage;

    [Header("Locomotive UI")]
    [SerializeField] private Image fuelFillImage;
    [SerializeField] private Image fuelMaxCapacityImage;
    [SerializeField] private Image shieldImage;

    [Header("Inventory UI")]
    [SerializeField] private Image CoalImage;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateWagonUI();
        UpdateLocomotiveUI();
        UpdateCoalUI();
    }

    void UpdateWagonUI()
    {
        WagonBrain wagon = playerInteractions.CurrentWagon;

        if (wagon != null)
        {
            wagonHpImage.gameObject.SetActive(true);
            wagonHpImage.fillAmount = wagon.CurrentHp / wagon.MaxHp;
        }
        else
        {
            wagonHpImage.gameObject.SetActive(false);
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
            CoalImage.color = inventory.HasCoal ? Color.black : Color.white;
        }

    }
}
