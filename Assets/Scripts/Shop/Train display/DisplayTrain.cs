using System.Collections.Generic;
using UnityEngine;

public class DisplayTrain : MonoBehaviour
{
    [SerializeField] private Transform currentTail;

    private List<IWagonID> wagonList;

    [SerializeField] private List<WagonInStockSO> wagonAssets;

    private Dictionary<string, GameObject> wagonAssetsReference;

    private void Start()
    {
        wagonAssetsReference = new Dictionary<string, GameObject>();

        foreach (var asset in wagonAssets)
        {
            wagonAssetsReference.Add(asset.wagonName, asset.shopModel);
        }

        wagonList = StoreManager.Instance.wagonsInTrain;

        foreach (var wagon in wagonList)
        {
            CreateWagon(wagonAssetsReference[wagon.WagonName]);
        }
    }

    // Ahora devuelve el GameObject creado.
    private GameObject CreateWagon(GameObject wagonModel)
    {
        GameObject currentWagon = Instantiate(
            wagonModel,
            currentTail.position,
            currentTail.rotation
        );

        Transform newTail = currentWagon.GetComponent<ShopWagonData>().tail;
        currentTail = newTail;

        return currentWagon;
    }

    // También devuelve el modelo nuevo para animarlo desde WagonShopButton.
    public GameObject AddWagon(WagonInStockSO wagonID)
    {
        wagonList.Add(new WagonStore(wagonID.Wagon, wagonID.wagonName));

        return CreateWagon(wagonID.shopModel);
    }

    public List<IWagonID> ChangeWagonIDList()
    {
        return wagonList;
    }
}