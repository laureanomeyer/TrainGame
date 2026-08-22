using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DisplayTrain : MonoBehaviour
{
    [SerializeField] private Transform currentTail;

    [Header("Wagon Spacing")]
    private float wagonGap = -0.6f;

    private LinkedList<IWagonID> wagonList;

    [SerializeField] private List<WagonInStockSO> wagonAssets;

    private Dictionary<string, GameObject> wagonAssetsReference;
    private Dictionary<int, ShopWagonData> instantiatedWagonReferences;

    private ICinematicActorRegistry cinematicActorRegistry;

    private readonly List<string> registeredKeys = new();
    private int wagonCounter;

    private Vector3 tailPos;
    private Quaternion tailRot;
    public Dictionary<int, ShopWagonData> InstantiatedWagonReferences => instantiatedWagonReferences;

    private void Awake()
    {
        cinematicActorRegistry = ServiceLocator.Get<ICinematicActorRegistry>();
    }

    private void Start()
    {
        wagonAssetsReference = new Dictionary<string, GameObject>();
        instantiatedWagonReferences = new Dictionary<int, ShopWagonData>();

        foreach (var asset in wagonAssets)
        {
            wagonAssetsReference.Add(asset.wagonName, asset.shopModel);
        }

        wagonList = new LinkedList<IWagonID>();

        foreach (var wagon in StoreManager.Instance.wagonsInTrain)
        {
            wagonList.AddLast(wagon);
        }

        tailPos = currentTail.position;
        tailRot = currentTail.rotation;

        int counter = 0;
        foreach (var wagon in wagonList)
        {
            instantiatedWagonReferences.Add(counter, CreateWagon(wagonAssetsReference[wagon.WagonName], wagon).Item2);

            counter++;
        }

        ServiceLocator.Register(this);
    }

    private (GameObject, ShopWagonData) CreateWagon(GameObject wagonModel, IWagonID data)
    {
        Vector3 spawnPosition = tailPos - (tailRot * Vector3.forward) * wagonGap;

        GameObject currentWagon = Instantiate(wagonModel, spawnPosition, tailRot);

        ShopWagonData wagonData = currentWagon.GetComponent<ShopWagonData>();
        wagonData.SetID(data);
        Transform newTail = wagonData.tail;

        tailPos = newTail.position;
        tailRot = newTail.rotation;

        return (currentWagon, wagonData);
    }

    public GameObject AddWagon(WagonInStockSO wagonID)
    {
        var newWag = new WagonStore(wagonID.Wagon, wagonID.wagonName);
        wagonList.AddLast(newWag);
        var newVisualWagon = CreateWagon(wagonID.shopModel, newWag);
        GameObject newWagon = newVisualWagon.Item1;


        if (instantiatedWagonReferences.Count == 0) instantiatedWagonReferences.Add(0, newVisualWagon.Item2);
        else instantiatedWagonReferences.Add(instantiatedWagonReferences.Count, newVisualWagon.Item2);

        string key = $"shop_wagon_{wagonCounter++}";
        registeredKeys.Add(key);

        cinematicActorRegistry.RegisterDynamic(key, newWagon.transform);
        EventBus.Publish(new OnWagonAddedToDisplayEvent(key));

        return newWagon;
    }

    public void ReorderWagons(ShopWagonData selected, ShopWagonData objective)
    {
        Vector3 reference = selected.transform.position;

        selected.transform.position = objective.transform.position;

        objective.transform.position = reference;

        var wagonA = wagonList.Find(selected.IDReference);
        var wagonB = wagonList.Find(objective.IDReference);

        if (wagonA != null && wagonB != null)
        {
            (wagonA.Value, wagonB.Value) = (wagonB.Value, wagonA.Value);
        }
        if (wagonList.Last == wagonA)
        {
            tailPos = objective.tail.position;
            tailRot = objective.tail.rotation;
        }
        else if (wagonList.Last == wagonB)
        {
            tailPos = selected.tail.position;
            tailRot = selected.tail.rotation;
        }
        else return;
    }

    public List<IWagonID> ChangeWagonIDList()
    {
        return wagonList.ToList();
    }

    private void OnDestroy()
    {
        foreach (var key in registeredKeys)
            cinematicActorRegistry?.UnregisterDynamic(key);

        registeredKeys.Clear();
    }
}