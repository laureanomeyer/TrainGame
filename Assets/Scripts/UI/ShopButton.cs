using System;
using UnityEngine;

public class ShopButton : MonoBehaviour
{
    [SerializeField] private string buttonText;
    [SerializeField] private WagonInStockSO[] wagonsInStock;

    private WagonInStockSO currentWagonInStock;
    public WagonInStockSO CurrentWagonInStock => currentWagonInStock;

    public string ButtonText => buttonText;

    [SerializeField] private WagonStoreManager storeManager;

    private void Start()
    {
        if (wagonsInStock.Length > 0)
        {
            SetWagonInStock();
        }
        else
        {
            string closeText = "No hay vagones actualmente. \n\n¡Vuelva Pronto!";
            buttonText = closeText;
        }
    }

    public void Interact()
    {
        if (wagonsInStock.Length > 0)
        {
            if (storeManager.ShowPlayerGold() < 10f)
            {
                Debug.Log("No se posee el dinero suficiente");
            }
            else
            {
                storeManager.ConsumeGold(10f);
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
        buttonText = (currentWagonInStock.Wagon.name + "\n\n" + "$" + currentWagonInStock.Price + "\n\n" + currentWagonInStock.Description);
    }

    private WagonInStockSO SelectRandomWagon()
    {
        int selector = UnityEngine.Random.Range(0, wagonsInStock.Length);
        WagonInStockSO wagonSelected = wagonsInStock[selector];

        return wagonSelected;
    }
}