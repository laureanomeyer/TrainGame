using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponShopButton : MonoBehaviour, IShopButton
{
    public PlayerAttackController PlayerReference { get => playerReference; set => playerReference = value; }
    private PlayerAttackController playerReference;
    public WeaponShopButtonManager ButtonManager { get => buttonManager; set => buttonManager = value; }
    private WeaponShopButtonManager buttonManager;
    public int Level { get => level; set => level = value; }
    private int level;
    public GameObject[] WeaponInStock { get => weaponInStock; set => weaponInStock = value; }
    public GameObject[] weaponInStock;

    private GameObject currentWeapon;

    [SerializeField] private TextMeshProUGUI buttonText;
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        if (level > weaponInStock.Length)
        {
            currentWeapon = weaponInStock[weaponInStock.Length - 1]; 
        }
        else
        {
            currentWeapon = weaponInStock[level - 1];
        }

        buttonText.text = currentWeapon.name;

        button.onClick.AddListener(BuyWeapon);
    }

    private void BuyWeapon()
    {
        playerReference.SetWeapon(currentWeapon);
        buttonManager.UpdateButtons(this);
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
