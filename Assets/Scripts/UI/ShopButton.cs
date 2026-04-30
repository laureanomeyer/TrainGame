using System;
using TMPro;
using UnityEngine;

public class ShopButton : MonoBehaviour
{
    [SerializeField] private string descriptionText;
    [SerializeField] private WagonInStockSO[] wagonsInStock;

    private WagonInStockSO currentWagonInStock;
    public WagonInStockSO CurrentWagonInStock => currentWagonInStock;

    public string DescriptionText => descriptionText;

    [SerializeField] private WagonStoreManager storeManager;

    private TextMeshProUGUI textUI;

    private void Start()
    {
        textUI = storeManager.descriptionTextUI;

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
            SetWagonInStock();
        }
    }

    private void SetWagonInStock()
    {
        currentWagonInStock = SelectRandomWagon();
        descriptionText = (currentWagonInStock.Name + "\n\n" + "$" + currentWagonInStock.Price + "\n\n" + currentWagonInStock.Description);
        textUI.text = descriptionText;
    }

    private WagonInStockSO SelectRandomWagon()
    {
        int selector = UnityEngine.Random.Range(0, wagonsInStock.Length);
        WagonInStockSO wagonSelected = wagonsInStock[selector];

        return wagonSelected;
    }
}