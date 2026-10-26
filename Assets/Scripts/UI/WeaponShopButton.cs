using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponShopButton : MonoBehaviour, IWeaponShopButton
{
    private PlayerBrain playerReference;
    private PlayerData playerDataRef;
    private int level;

    private int selectedWeapon;
    private GameObject currentWeapon;
    private float currentWeaponprice = 0;

    [Header("Button UI")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI priceText;

    [Header("Weapon stats UI")]
    [SerializeField] private TextMeshProUGUI[] damageText;
    [SerializeField] private TextMeshProUGUI[] rofText;
    [SerializeField] private TextMeshProUGUI[] ammunitionText;

    [Header("Weapon image UI")]
    [SerializeField] private Image[] weaponImage;

    [Header("Legacy data UI")]
    [SerializeField] private TextMeshProUGUI legacyName;
    [SerializeField] private CanvasGroup legacyStar;
    [SerializeField] private TextMeshProUGUI legacyUnlockCondition;
    [SerializeField] private TextMeshProUGUI legacyDescription;
    [SerializeField] private CanvasGroup legacyLocked;

    [SerializeField] private Button buyButton;

    [Header("Shake Feedback")]
    [SerializeField] private float shakeDistance = 15f;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private int shakeCount = 10;


    public PlayerBrain PlayerReference { get => playerReference; set => playerReference = value; }
    public WeaponShopButtonManager ButtonManager { get => buttonManager; set => buttonManager = value; }
    private WeaponShopButtonManager buttonManager;
    public int Level { get => level; set => level = value; }

    [Header("Weapon collection")]
    public WeaponCollectionInStockSO[] collections;

    private Dictionary<int, WeaponInStocSO[]> weaponCollections = new Dictionary<int, WeaponInStocSO[]>();
    private WeaponInStocSO[] currentCollection;

    private Tweener shakeTween;
    private Vector2 buyButtonBasePos;

    private void Awake()
    {
        playerDataRef = ServiceLocator.Get<PlayerData>();
        buyButton.onClick.AddListener(BuyWeapon);
    }
    public void SetWeapon()
    {
        int value = UnityEngine.Random.Range(0, currentCollection.Length);
        selectedWeapon = value;

        currentWeapon = currentCollection[selectedWeapon].Weapon;
        currentWeaponprice = currentCollection[selectedWeapon].Price;

        UpdateInfo(currentCollection[selectedWeapon]);

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
                if (collection == null)
                {
                    Debug.LogWarning($"Slot null en 'collections' del objeto {name}, revisar el Inspector.", this);
                    continue;
                }

                if (!weaponCollections.ContainsKey(collection.Level))
                {
                    weaponCollections.Add(collection.Level, collection.weaponCollection);
                }
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
                return;
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
            ShakeBuyButton(shakeDistance, shakeDuration, shakeCount);
        }
        else
        {
            Sprite boughtWeaponSprite = currentCollection[selectedWeapon].WeaponSprite;

            playerDataRef.ChangeWeaponData(currentWeapon);
            playerReference.ChangeWeapon(currentWeapon);
            EventBus.Publish(new OnWeaponChangedEvent(boughtWeaponSprite));
            DeactivateButton();

            MoveOutOfView();
        }
    }

    private void UpdateInfo(WeaponInStocSO stockInfo)
    {
        currentWeapon = stockInfo.Weapon;
        currentWeaponprice = stockInfo.Price;

        weaponNameText.text = currentWeapon.name;
        priceText.text = ("Buy for $" + currentWeaponprice.ToString());


        float cooldown = (stockInfo.WeaponData.rateOfFire + stockInfo.WeaponData.reloadTime) / 2;
        float damage = stockInfo.WeaponData.damage / cooldown;

        foreach (var t in damageText)
            t.text = FormatStat(damage);

        foreach (var t in ammunitionText)
            t.text = FormatStat (stockInfo.WeaponData.ammun);

        foreach (var t in rofText)
            t.text = FormatStat(cooldown);

        foreach (var t in weaponImage)
            t.sprite = stockInfo.WeaponSprite;


        if(stockInfo is WeaponWithLegacyInStockSO)
        {
            WeaponWithLegacyInStockSO legacyWeapon = stockInfo as WeaponWithLegacyInStockSO;

            legacyName.text = legacyWeapon.legacyName;
            legacyUnlockCondition.text = legacyWeapon.legacyUnlockDescription;
            legacyDescription.text = legacyWeapon.legacyDescription;

            if (legacyWeapon.CheckUnlockLegacy())
            {
                legacyStar.alpha = 1f;
                legacyLocked.alpha = 0f;
            }
            else
            {
                legacyStar.alpha = 0f;
                legacyLocked.alpha = 1f;
            }
        }
        else
        {
            legacyName.text = "Basic";
            legacyUnlockCondition.text = "This weapon does not have a legacy";
            legacyDescription.text = "";
            legacyLocked.alpha = 0f;

            legacyStar.alpha = 0f;
        }
    }

    public void ActivateButton()
    {
        if (currentCollection == null || currentCollection.Length == 0) return;

        UpdateInfo(currentCollection[selectedWeapon]);

        if (playerDataRef.PlayerWeapon == currentWeapon)
            DeactivateButton();
        else
            buyButton.interactable = true;
    }

    public void DeactivateButton()
    {
        buyButton.interactable = false;
        priceText.text = "Owned";
    }

    private string FormatStat(float value)
    {
        return (value % 1 == 0) ? value.ToString("F0") : value.ToString("F1");
    }

    public void ShakeBuyButton(float distance, float duration, int vibrato)
    {
        Debug.Log("Shakeo " + distance);
        var rect = (RectTransform)buyButton.transform;

        if (shakeTween.IsActive())
        {
            shakeTween.Kill();
            rect.anchoredPosition = buyButtonBasePos;
        }
        else
        {
            buyButtonBasePos = rect.anchoredPosition;
        }

        shakeTween = rect.DOShakeAnchorPos(duration, new Vector2(distance, 0f), vibrato, 0f, false, true).SetUpdate(true).SetLink(gameObject).OnKill(() => rect.anchoredPosition = buyButtonBasePos);
    }

    public void MoveOutOfView()
    {
        StartCoroutine(MoverRoutine());
    }

    private IEnumerator MoverRoutine()
    {
        RectTransform rect = (RectTransform)transform;
        Vector2 start = rect.anchoredPosition;
        Vector2 outPosition = new Vector2(start.x - 2000f, start.y); 

        float t = 0f;
        float duration = 0.4f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            rect.anchoredPosition = Vector2.Lerp(start, outPosition, t);
            yield return null; 
        }

        rect.anchoredPosition = outPosition;
        gameObject.SetActive(false);
    }
}
