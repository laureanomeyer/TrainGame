using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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

    [Header("Weapon stats UI")]
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI rofText;
    [SerializeField] private TextMeshProUGUI ammunitionText;

    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Weapon image UI")]
    [SerializeField] private Image weaponImage;

    [Header("Legacy data UI")]
    [SerializeField] private GameObject legacyStar;
    [SerializeField] private TextMeshProUGUI legacyDescription;

    private Button button;
    public PlayerBrain PlayerReference { get => playerReference; set => playerReference = value; }
    public WeaponShopButtonManager ButtonManager { get => buttonManager; set => buttonManager = value; }
    private WeaponShopButtonManager buttonManager;
    public int Level { get => level; set => level = value; }

    public WeaponInStocSO[] WeaponInStock { get => weaponInStock; set => weaponInStock = value; }
    public WeaponInStocSO[] weaponInStock;

    [Header("Weapon collection")]
    public WeaponCollectionInStockSO[] collections;

    private Dictionary<int, WeaponInStocSO[]> weaponCollections = new Dictionary<int, WeaponInStocSO[]>();
    private WeaponInStocSO[] currentCollection;

    private void Awake()
    {
        playerDataRef = ServiceLocator.Get<PlayerData>();

        button = GetComponent<Button>();
        button.onClick.AddListener(BuyWeapon);
    }
    public void SetWeapon()
    {
        int value = UnityEngine.Random.Range(0, currentCollection.Length);

        Debug.Log(value);

        currentWeapon = currentCollection[value].Weapon;
        currentWeaponprice = currentCollection[value].Price;

        UpdateInfo(currentCollection[value]);

        if (playerDataRef.PlayerWeapon == currentWeapon)
        {
            DeactivateButton();
        }
    }

    public void SetValues(int level)
    {
        if (collections.Count() > 0 && level > 0)
        {
            foreach (WeaponCollectionInStockSO collection in collections)
            {
                weaponCollections.Add(collection.Level, collection.weaponCollection);
            }

            if (weaponCollections.ContainsKey(level))
            {
                currentCollection = weaponCollections[level];
            }
            else if (level >= weaponCollections.Count)
            {
                currentCollection = weaponCollections[weaponCollections.Count];
            }
            else
            {
                Debug.Log("Llave no encontrada");
                DeactivateButton();
                this.gameObject.SetActive(false);
            }
            

            if (currentCollection.Length > 0)
            {
                SetWeapon();
            }
            else
            {
                Debug.Log("Sin elementos en la lista");
                DeactivateButton();
                this.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log("Sin elementos en el diccionario");

            DeactivateButton();
            this.gameObject.SetActive(false);
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

            SetWeapon();
        }
    }

    private void UpdateInfo(WeaponInStocSO stockInfo)
    {
        currentWeapon = stockInfo.Weapon;
        currentWeaponprice = stockInfo.Price;

        weaponNameText.text = currentWeapon.name;
        priceText.text = currentWeaponprice.ToString() + "$";

        float cooldown = (stockInfo.WeaponData.rateOfFire + stockInfo.WeaponData.reloadTime) / 2;
        float damage = stockInfo.WeaponData.damage / cooldown;

        damageText.text = FormatStat(damage);
        ammunitionText.text = FormatStat (stockInfo.WeaponData.ammun);
        rofText.text = FormatStat(cooldown);

        weaponImage.sprite = stockInfo.GunSprite;

        if(stockInfo is WeaponWithLegacyInStockSO)
        {
            WeaponWithLegacyInStockSO legacyWeapon = stockInfo as WeaponWithLegacyInStockSO;

            legacyDescription.text = legacyWeapon.legacyDescription + "\n" + legacyWeapon.legacyUnlockDescription;

            legacyStar.SetActive(legacyWeapon.CheckUnlockLegacy());
        }
        else
        {
            legacyDescription.text = "";

            legacyStar.SetActive(false);
        }
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

    private string FormatStat(float value)
    {
        return (value % 1 == 0) ? value.ToString("F0") : value.ToString("F1");
    }
}
