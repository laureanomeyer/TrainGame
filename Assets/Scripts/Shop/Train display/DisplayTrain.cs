using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DisplayTrain : MonoBehaviour
{
    [SerializeField] private Transform currentTail;

    private List<IWagonID> wagonList;

    [SerializeField] private List<WagonInStockSO> wagonAssets;

    private Dictionary<string , GameObject> wagonAssetsReference;

    private void Start()
    {
        wagonAssetsReference = new Dictionary<string, GameObject>();

        if (wagonAssets.Count > 0)
        {
            foreach (var asset in wagonAssets)
            {
                wagonAssetsReference.Add(asset.wagonName, asset.shopModel);
            }
        }

        wagonList = StoreManager.Instance.wagonsInTrain;

        if (wagonList.Count > 0)
        {
            foreach (var wagon in wagonList)
            {
                CreateWagon(wagonAssetsReference[wagon.WagonName]);
            }
        }
    }

    private void CreateWagon(GameObject wagonModel)
    {
        GameObject currentWagon = Instantiate(wagonModel, currentTail.position, currentTail.rotation);

        Transform tail = currentWagon.GetComponent<ShopWagonData>().tail;
        currentTail = tail;
    }

    public void AddWagon(WagonInStockSO wagonID)
    {
        wagonList.Add(new WagonStore(wagonID.Wagon, wagonID.wagonName));
        CreateWagon(wagonID.shopModel);
    }

    public List<IWagonID> ChangeWagonIDList()
    {
        return wagonList;
    }
}
