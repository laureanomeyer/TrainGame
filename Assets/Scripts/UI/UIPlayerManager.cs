using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIPlayerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteractions playerInteractions;

    [Header("UI Groups")]
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject shopUI;

    [Header("Wagon UI")]
    [SerializeField] private Image wagonHpImage;

    [Header("Locomotive UI")]
    [SerializeField] private Image fuelFillImage;
    [SerializeField] private Image fuelMaxCapacityImage;
    [SerializeField] private Image shieldImage;

    [Header("Inventory UI")]
    [SerializeField] private Image coalImage;
    [SerializeField] private TMP_Text goldText;

    private void Start()
    {
        UpdateUIByScene();
    }

    private void Update()
    {
        UpdateUIByScene();

        if (shopUI.activeSelf)
        {
            UpdateGoldUI();
        }
        else
        {
            UpdateWagonUI();
            UpdateLocomotiveUI();
            UpdateCoalUI();
            UpdateGoldUI(); // opcional, si también querés ver oro durante gameplay
        }
    }

    void UpdateUIByScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        bool isShopScene = sceneName == "ShopScene"; // cambiá esto por el nombre real

        gameplayUI.SetActive(!isShopScene);
        shopUI.SetActive(isShopScene);
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
            coalImage.fillAmount = inventory.HasCoal ? 1 : 0;
        }
    }

    void UpdateGoldUI()
    {
        PlayerInventory inventory = playerInteractions.Inventory;

        if (inventory != null)
        {
            goldText.text = "Gold: " + inventory.GoldAmount.ToString("0");
        }
    }
}