using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponShopButton : MonoBehaviour, IShopButton
{
    private PlayerBrain playerReference;
    private int level;
    private PlayerData playerDataRef;
    private GameObject currentWeapon;
    private float currentWeaponprice = 0;

    [Header("Button UI")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI rofText;
    [SerializeField] private TextMeshProUGUI ammunitionText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image weaponImage;

    private Button button;
    public PlayerBrain PlayerReference { get => playerReference; set => playerReference = value; }
    public WeaponShopButtonManager ButtonManager { get => buttonManager; set => buttonManager = value; }
    private WeaponShopButtonManager buttonManager;
    public int Level { get => level; set => level = value; }
    public WeaponInStocSO[] WeaponInStock { get => weaponInStock; set => weaponInStock = value; }
    public WeaponInStocSO[] weaponInStock;

    private void Awake()
    {
        playerDataRef = ServiceLocator.Get<PlayerData>();
    }
    public void SetValuesInStock()
    {
        button = GetComponent<Button>();

        if (level > weaponInStock.Length)
        {
            currentWeapon = weaponInStock[weaponInStock.Length].Weapon;
            currentWeaponprice = weaponInStock[weaponInStock.Length].Price;

            UpdateInfo(weaponInStock[weaponInStock.Length]);
        }
        else
        {
            currentWeapon = weaponInStock[level - 1].Weapon;
            currentWeaponprice = weaponInStock[level - 1].Price;

            UpdateInfo(weaponInStock[level - 1]);
        }

        button.onClick.AddListener(BuyWeapon);

        if (playerDataRef.PlayerWeapon == currentWeapon)
        {
            DeactivateButton();
        }
    }

    private void BuyWeapon()
    {
        if (!buttonManager.TryConsumeGold(currentWeaponprice))
        {
            Debug.Log("No se posee el dinero suficiente");
        }
        else
        {
            playerDataRef.ChangeWeaponData(currentWeapon);
            playerReference.ChangeWeapon(currentWeapon);
            buttonManager.UpdateButtons(this);
        }
    }

    private void UpdateInfo(WeaponInStocSO stockInfo)
    {
        currentWeapon = stockInfo.Weapon;
        currentWeaponprice = stockInfo.Price;

        weaponNameText.text = currentWeapon.name;
        priceText.text = currentWeaponprice.ToString() + "$";

        damageText.text = stockInfo.WeaponData.damage.ToString();
        ammunitionText.text = stockInfo.WeaponData.ammun.ToString();
        rofText.text = stockInfo.WeaponData.rateOfFire.ToString();

        weaponImage.sprite = stockInfo.GunSprite;
    }

    public void ActivateButton()
    {
        button.interactable = true;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
    }

    public void DeactivateButton()
    {
        button.interactable = false;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
    }
}
