using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponShopButton : MonoBehaviour, IShopButton
{
    public PlayerBrain PlayerReference { get => playerReference; set => playerReference = value; }
    private PlayerBrain playerReference;
    public WeaponShopButtonManager ButtonManager { get => buttonManager; set => buttonManager = value; }
    private WeaponShopButtonManager buttonManager;
    public int Level { get => level; set => level = value; }
    private int level;
    public WeaponInStocSO[] WeaponInStock { get => weaponInStock; set => weaponInStock = value; }
    public WeaponInStocSO[] weaponInStock;

    private GameObject currentWeapon;
    private float currentWeaponprice = 0;

    [SerializeField] private TextMeshProUGUI buttonText;
    private Button button;

    public void SetValuesInStock()
    {
        button = GetComponent<Button>();

        if (level > weaponInStock.Length)
        {
            currentWeapon = weaponInStock[weaponInStock.Length].Weapon;
            currentWeaponprice = weaponInStock[weaponInStock.Length].Price;
        }
        else
        {
            currentWeapon = weaponInStock[level - 1].Weapon;
            currentWeaponprice = weaponInStock[level - 1].Price;
        }

        buttonText.text = currentWeapon.name + "   $" + currentWeaponprice;

        button.onClick.AddListener(BuyWeapon);
    }

    private void BuyWeapon()
    {
        if (!buttonManager.TryConsumeGold(currentWeaponprice))
        {
            Debug.Log("No se posee el dinero suficiente");
        }
        else
        {
            //GameManager.Instance.Session.PlayerData.ChangeWeaponData(currentWeapon);
            //playerReference.ChangeWeapon(currentWeapon);
            buttonManager.UpdateButtons(this);
        }
       
    }

    public void ActivateButton()
    {
        button.interactable = true;
    }

    public void DeactivateButton()
    {
        button.interactable = false;
    }
}
