using System.Collections.Generic;
using UnityEngine;

public class DisplayTrain : MonoBehaviour
{
    [SerializeField] private Transform currentTail;

    [Header("Wagon Spacing")]
    private float wagonGap = -0.6f;

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

    private GameObject CreateWagon(GameObject wagonModel)
    {
        Vector3 spawnPosition = currentTail.position - currentTail.forward * wagonGap;

        GameObject currentWagon = Instantiate(
            wagonModel,
            spawnPosition,
            currentTail.rotation
        );

        Transform newTail = currentWagon.GetComponent<ShopWagonData>().tail;
        currentTail = newTail;

        return currentWagon;
    }

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