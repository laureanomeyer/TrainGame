using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopButton : MonoBehaviour
{
    private string descriptionText;

    private int level;

    [SerializeField] private WagonInStockSO[] wagonsInStock;

    private WagonInStockSO currentWagonInStock;
    public WagonInStockSO CurrentWagonInStock => currentWagonInStock;

    [SerializeField] private ShopWagonCollectionSO[] wagonsCollectionSOs;

    private Dictionary<int, WagonInStockSO[]> wagonsCollections = new Dictionary<int, WagonInStockSO[]>();

    [SerializeField] private Transform spawnWagonPoint;

    private GameObject modelReference;

    public string DescriptionText => descriptionText;

    [SerializeField] private WagonStoreManager storeManager;

    private TextMeshProUGUI textUI;

    private void Start()
    {
        textUI = storeManager.descriptionTextUI;

        level = storeManager.Level;

        foreach (var collection in wagonsCollectionSOs)
        {
            wagonsCollections.Add(collection.level, collection.WagonCollection);
        }


        if (level > wagonsCollections.Count)
        {
            wagonsInStock = wagonsCollections[wagonsCollections.Count];
        }
        else
        {
            if (wagonsCollections.ContainsKey(level))
            {
                wagonsInStock = wagonsCollections[level];
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