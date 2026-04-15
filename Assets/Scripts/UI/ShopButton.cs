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
        if (wagonsInStock.Length > 0)
        {
            if (storeManager.ShowPlayerGold() < currentWagonInStock.Price)
            {
                Debug.Log("No se posee el dinero suficiente");
            }
            else
            {
                storeManager.ConsumeGold(currentWagonInStock.Price);
                GameManager.Instance.TrainData.AddWagonID(new WagonStore(currentWagonInStock.Wagon));
                SetWagonInStock();
            }
        }
        else
        {
            Debug.Log("No hay objetos en Stock");
        }
    }

    private void SetWagonInStock()
    {
        currentWagonInStock = SelectRandomWagon();
        Debug.Log(currentWagonInStock.name);
        descriptionText = (currentWagonInStock.Wagon.name + "\n\n" + "$" + currentWagonInStock.Price + "\n\n" + currentWagonInStock.Description);
        textUI.text = descriptionText;
    }

    private WagonInStockSO SelectRandomWagon()
    {
        int selector = UnityEngine.Random.Range(0, wagonsInStock.Length);
        WagonInStockSO wagonSelected = wagonsInStock[selector];

        return wagonSelected;
    }
}