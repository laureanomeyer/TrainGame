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

    private ICinematicActorRegistry cinematicActorRegistry;

    private readonly List<string> registeredKeys = new();
    private int wagonCounter;

    private Vector3 tailPos;
    private Quaternion tailRot;

    private void Awake()
    {
        cinematicActorRegistry = ServiceLocator.Get<ICinematicActorRegistry>();
    }

    private void Start()
    {
        wagonAssetsReference = new Dictionary<string, GameObject>();

        foreach (var asset in wagonAssets)
        {
            wagonAssetsReference.Add(asset.wagonName, asset.shopModel);
        }

        wagonList = StoreManager.Instance.wagonsInTrain;

        tailPos = currentTail.position;
        tailRot = currentTail.rotation;

        foreach (var wagon in wagonList)
        {
            CreateWagon(wagonAssetsReference[wagon.WagonName]);
        }
    }

    private GameObject CreateWagon(GameObject wagonModel)
    {
        Vector3 spawnPosition = tailPos - (tailRot * Vector3.forward) * wagonGap;

        GameObject currentWagon = Instantiate(
            wagonModel,
            spawnPosition,
            tailRot
        );

        Transform newTail = currentWagon.GetComponent<ShopWagonData>().tail;

        tailPos = newTail.position;
        tailRot = newTail.rotation;

        return currentWagon;
    }

    public GameObject AddWagon(WagonInStockSO wagonID)
    {
        wagonList.Add(new WagonStore(wagonID.Wagon, wagonID.wagonName));
        GameObject newWagon = CreateWagon(wagonID.shopModel);

        string key = $"shop_wagon_{wagonCounter++}";
        registeredKeys.Add(key);

        cinematicActorRegistry.RegisterDynamic(key, newWagon.transform);
        EventBus.Publish(new OnWagonAddedToDisplayEvent(key));

        return newWagon;
    }

    public List<IWagonID> ChangeWagonIDList()
    {
        return wagonList;
    }

    private void OnDestroy()
    {
        foreach (var key in registeredKeys)
            cinematicActorRegistry?.UnregisterDynamic(key);

        registeredKeys.Clear();
    }
}