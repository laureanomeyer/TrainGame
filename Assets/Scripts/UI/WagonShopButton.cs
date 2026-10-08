
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WagonShopButton : MonoBehaviour
{
    private string descriptionText;

    private int currentLevel;

    public int Level { get => currentLevel; set => currentLevel = value; }

    public DisplayTrain displayTrain;

    public Button buyButton;

    private RectTransform buyButtonRect;
    private Vector2 buttonOriginalPos;

    private WagonInStockSO[] wagonsInStock;

    private WagonInStockSO currentWagonInStock;
    public WagonInStockSO CurrentWagonInStock => currentWagonInStock;

    [SerializeField] private ShopWagonCollectionSO[] wagonsCollectionSOs;

    private Dictionary<int, WagonInStockSO[]> wagonsCollections = new Dictionary<int, WagonInStockSO[]>();

    [SerializeField] private Transform spawnWagonPoint;

    [SerializeField] private ParticleSystem wagonShopParticleSystem;

    private GameObject modelReference;

    public string DescriptionText => descriptionText;

    [SerializeField] private WagonShopManager storeManager;

    private TextMeshProUGUI nameTextUI;
    private TextMeshProUGUI descriptionTextUI;
    private TextMeshProUGUI priceTextUI;

    private InteractionZone interacZone;

    private bool canDoReroll = true;
    private bool usedReroll = false;


    /*[Header("Wagon Arrival Animation")]
    [SerializeField] private float wagonArrivalDuration = 1.5f;

    [Tooltip("Distancia desde la izquierda donde aparece antes de entrar.")]
    [SerializeField] private float wagonStartOffsetX = -35f;
    */

    [SerializeField] private float waitTimeToBuy;

    private bool isBuyingWagon;

    [Header("Shake Feedback")]
    [SerializeField] private float shakeDistance = 15f;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private int shakeCount = 3;

    private Coroutine shakeCoroutine;

    public void Initialize()
    {
        nameTextUI = storeManager.nameTextUI;
        descriptionTextUI = storeManager.descriptionTextUI;
        priceTextUI = storeManager.priceTextUI;

        buyButtonRect = buyButton.GetComponent<RectTransform>();
        buttonOriginalPos = buyButtonRect.anchoredPosition;

        interacZone = GetComponent<InteractionZone>();

        foreach (var collection in wagonsCollectionSOs)
        {
            wagonsCollections.Add(collection.level, collection.WagonCollection);
        }


        if (currentLevel > wagonsCollections.Count)
        {
            wagonsInStock = wagonsCollections[wagonsCollections.Count];
        }
        else
        {
            if (wagonsCollections.ContainsKey(currentLevel))
            {
                wagonsInStock = wagonsCollections[currentLevel];
            }
            else
            {
                wagonsInStock = new WagonInStockSO[0];
            }
        }

        if (wagonsInStock.Length > 0)
        {
            SetWagonInStock(false);

            storeManager.ActivateButtons();
        }
        else
        {
            nameTextUI.text = null;

            string closeText = "No hay vagones actualmente. \n\n¡Vuelva Pronto!";
            descriptionText = closeText;

            canDoReroll = false;

            priceTextUI.text = null;

            storeManager.DeactivateButtons();
        }
    }

    public void Interact()
    {
        if (wagonsInStock.Length == 0)
            return;

        if (isBuyingWagon)
            return;

        if (!storeManager.TryConsumeGold(currentWagonInStock.Price))
        {
            PlayShake();
            return;
        }

        StartCoroutine(BuyWagonCoroutine());
    }

    private void SetWagonInStock(bool notRepeat)
    {
        if(modelReference != null)
        {
            Destroy(modelReference);
        }

        currentWagonInStock = SelectRandomWagon(notRepeat);

        nameTextUI.text = currentWagonInStock.wagonName;

        descriptionText = (currentWagonInStock.Description);
        descriptionTextUI.text = descriptionText;

        priceTextUI.text = "$" + currentWagonInStock.Price.ToString() ;

        modelReference = Instantiate(currentWagonInStock.shopModel, spawnWagonPoint.position, spawnWagonPoint.rotation);
    }

    private WagonInStockSO SelectRandomWagon(bool notRepeat)
    {
        int selector = UnityEngine.Random.Range(0, wagonsInStock.Length);
        WagonInStockSO wagonSelected = wagonsInStock[selector];

        if (notRepeat)
        {
            if(wagonsInStock.Length > 1)
            {
                if (currentWagonInStock == wagonSelected)
                {
                    return SelectRandomWagon(true);
                }
                else
                {
                    return wagonSelected;
                }
            }
            else
            {
                return wagonSelected;
            }
        }
        else
        {
            return wagonSelected;
        }
    }

    public void UpdateUI()
    {
        if (wagonsInStock.Length > 0)
        {
            nameTextUI.text = currentWagonInStock.wagonName;

            descriptionText = (currentWagonInStock.Description);
            descriptionTextUI.text = descriptionText;

            priceTextUI.text = "$" + currentWagonInStock.Price.ToString();

            storeManager.ActivateButtons();
            CheckReroll();
        }
        else
        {
            nameTextUI.text = null;

            string closeText = "No hay vagones actualmente. \n\n¡Vuelva Pronto!";
            descriptionText = closeText;

            canDoReroll = false;

            priceTextUI.text = null;

            storeManager.DeactivateButtons();
        }
    }

    private void UsedReroll()
    {
        SetWagonInStock(true);

        if (wagonShopParticleSystem)
        {
            wagonShopParticleSystem.Play();
        }

        usedReroll = true;
        storeManager.rerollButton.interactable = false;
    }

    private void CheckReroll()
    {
        if (canDoReroll)
        {
            if (usedReroll && storeManager.rerollButton.interactable == true)
            {
                storeManager.rerollButton.interactable = false;
            }
            else
            {
                storeManager.rerollButton.interactable = true;
            }
        }
        else
        {
            if (storeManager.rerollButton.interactable == true)
            {
                storeManager.rerollButton.interactable = false;
            }
        }
    }

    public void CloseFuction()
    {
        interacZone.DeactivateUI();
    }

    private void PlayShake()
    {
        if (buyButtonRect == null) return;

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            // solo restauramos si había un shake en curso
            buyButtonRect.anchoredPosition = buttonOriginalPos;
        }
        else
        {
            // no hay shake activo: la posición actual es la buena
            buttonOriginalPos = buyButtonRect.anchoredPosition;
        }

        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    private void ResetShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        if (buyButtonRect != null)
        {
            buyButtonRect.anchoredPosition = buttonOriginalPos;
        }
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / shakeDuration;

            // Oscila izquierda-derecha y la amplitud baja a 0, así termina en el centro
            float offset = Mathf.Sin(t * shakeCount * Mathf.PI * 2f) * shakeDistance * (1f - t);
            buyButtonRect.anchoredPosition = buttonOriginalPos + new Vector2(offset, 0f);

            yield return null;
        }

        buyButtonRect.anchoredPosition = buttonOriginalPos;
        shakeCoroutine = null;
    }

    private void OnDisable()
    {
        ResetShake();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            storeManager.rerollButton.onClick.AddListener(UsedReroll);
            storeManager.buyButton.onClick.AddListener(Interact);

            storeManager.closeButton.onClick.AddListener(CloseFuction);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            storeManager.rerollButton.onClick.RemoveListener(UsedReroll);
            storeManager.buyButton.onClick.RemoveListener(Interact);

            storeManager.closeButton.onClick.RemoveListener(CloseFuction);

            ResetShake();
        }
    }

    //coroutine
    private IEnumerator BuyWagonCoroutine()
    {
        isBuyingWagon = true;
        interacZone.HideUI();

        storeManager.buyButton.interactable = false;
        storeManager.rerollButton.interactable = false;

        displayTrain.AddWagon(currentWagonInStock);

        GameManager.Instance.Session.RebuildStatsSystem();

        SetWagonInStock(false);

        if (wagonShopParticleSystem != null)
        {
            wagonShopParticleSystem.Play();
        }

        yield return new WaitForSeconds(waitTimeToBuy);

        isBuyingWagon = false;
        interacZone.ShowUi();

        storeManager.buyButton.interactable = true;

        CheckReroll();
    }
}