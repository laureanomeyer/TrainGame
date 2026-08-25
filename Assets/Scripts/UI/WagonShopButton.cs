using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;

public class WagonShopButton : MonoBehaviour
{
    private string descriptionText;

    private int currentLevel;

    public int Level { get => currentLevel; set => currentLevel = value; }

    public DisplayTrain displayTrain;

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


    [Header("Wagon Arrival Animation")]
    [SerializeField] private float wagonArrivalDuration = 1.5f;

    [Tooltip("Distancia desde la izquierda donde aparece antes de entrar.")]
    [SerializeField] private float wagonStartOffsetX = -35f;

    [SerializeField] private float waitTimeToBuy;

    private bool isBuyingWagon;

    private void Start()
    {
        nameTextUI = storeManager.nameTextUI;
        descriptionTextUI = storeManager.descriptionTextUI;
        priceTextUI = storeManager.priceTextUI;

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
            SetWagonInStock();

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
            return;

        StartCoroutine(BuyWagonCoroutine());
    }

    private void SetWagonInStock()
    {
        if(modelReference != null)
        {
            Destroy(modelReference);
        }

        currentWagonInStock = SelectRandomWagon();

        nameTextUI.text = currentWagonInStock.wagonName;

        descriptionText = (currentWagonInStock.Description);
        descriptionTextUI.text = descriptionText;

        priceTextUI.text = "$" + currentWagonInStock.Price.ToString() ;

        modelReference = Instantiate(currentWagonInStock.shopModel, spawnWagonPoint.position, spawnWagonPoint.rotation);
    }

    private WagonInStockSO SelectRandomWagon()
    {
        int selector = UnityEngine.Random.Range(0, wagonsInStock.Length);
        WagonInStockSO wagonSelected = wagonsInStock[selector];

        return wagonSelected;
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
        SetWagonInStock();

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
        }
    }

    //coroutine
    private IEnumerator BuyWagonCoroutine()
    {
        isBuyingWagon = true;


        storeManager.buyButton.interactable = false;
        storeManager.rerollButton.interactable = false;


        GameObject newWagon = displayTrain.AddWagon(currentWagonInStock);

        if (newWagon != null)
        {
            Vector3 finalPosition = newWagon.transform.position;
            Quaternion finalRotation = newWagon.transform.rotation;


            Vector3 startPosition = finalPosition;
            startPosition.x += wagonStartOffsetX;

            newWagon.transform.position = startPosition;
            newWagon.transform.rotation = finalRotation;

            float timer = 0f;

            while (timer < wagonArrivalDuration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / wagonArrivalDuration);


                t = t * t * (3f - 2f * t);

                newWagon.transform.position = Vector3.Lerp(startPosition, finalPosition, t);

                yield return null;
            }

            newWagon.transform.position = finalPosition;
        }

        GameManager.Instance.Session.RebuildStatsSystem();

        SetWagonInStock();

        if (wagonShopParticleSystem != null)
        {
            wagonShopParticleSystem.Play();
        }

        yield return new WaitForSeconds(waitTimeToBuy);

        isBuyingWagon = false;

        storeManager.buyButton.interactable = true;

        CheckReroll();
    }
}