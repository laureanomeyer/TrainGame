using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WagonShopButton : MonoBehaviour
{
    private string descriptionText;

    private int currentLevel;

    public int Level { get => currentLevel; set => currentLevel = value; }

    private WagonInStockSO[] wagonsInStock;

    private WagonInStockSO currentWagonInStock;
    public WagonInStockSO CurrentWagonInStock => currentWagonInStock;

    [SerializeField] private ShopWagonCollectionSO[] wagonsCollectionSOs;

    private Dictionary<int, WagonInStockSO[]> wagonsCollections = new Dictionary<int, WagonInStockSO[]>();

    [SerializeField] private Transform spawnWagonPoint;

    [SerializeField] private ParticleSystem wagonShopParticleSystem;

    private GameObject modelReference;

    public string DescriptionText => descriptionText;

    [SerializeField] private WagonStoreManager storeManager;

    private TextMeshProUGUI textUI;

    private void Start()
    {
        textUI = storeManager.descriptionTextUI;

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
        }
        else
        {
            string closeText = "No hay vagones actualmente. \n\n¡Vuelva Pronto!";
            descriptionText = closeText;
        }
    }

    public void Interact()
    {
        if (wagonsInStock.Length == 0) return;
        if (storeManager.TryConsumeGold(currentWagonInStock.Price))
        {
            GameManager.Instance.Session.TrainData.AddWagonID(new WagonStore(currentWagonInStock.Wagon));
            GameManager.Instance.Session.RebuildStatsSystem();
            Destroy(modelReference);
            SetWagonInStock();
            if (wagonShopParticleSystem)
            {
                wagonShopParticleSystem.Play();
            }
        }
    }

    private void SetWagonInStock()
    {
        currentWagonInStock = SelectRandomWagon();
        descriptionText = (currentWagonInStock.Name + "\n\n" + "$" + currentWagonInStock.Price + "\n\n" + currentWagonInStock.Description);
        textUI.text = descriptionText;

        modelReference = Instantiate(currentWagonInStock.shopModel, spawnWagonPoint.position, spawnWagonPoint.rotation);
    }

    private WagonInStockSO SelectRandomWagon()
    {
        int selector = UnityEngine.Random.Range(0, wagonsInStock.Length);
        WagonInStockSO wagonSelected = wagonsInStock[selector];

        return wagonSelected;
    }
}