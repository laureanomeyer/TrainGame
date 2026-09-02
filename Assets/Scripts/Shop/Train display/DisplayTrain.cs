using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class DisplayTrain : MonoBehaviour
{
    [SerializeField] private Transform currentTail;

    [Header("Wagon Spacing")]
    private float wagonGap = -0.6f;

    [Header("New Wagon Pop-In Animation")]
    [SerializeField] private float popInDelay = 0.5f;
    [SerializeField] private float popInDuration = 0.4f;
    [SerializeField] private Ease popInEase = Ease.OutBack;

    [Header("Existing Wagons Shift Animation")]
    [SerializeField] private float shiftDuration = 0.4f;
    [SerializeField] private Ease shiftEase = Ease.OutQuad;

    [SerializeField] private List<WagonInStockSO> wagonAssets;

    private Dictionary<string, GameObject> wagonAssetsReference;

    private LinkedList<IWagonID> wagonList;
    private Dictionary<int, ShopWagonData> instantiatedWagonReferences;

    private ICinematicActorRegistry cinematicActorRegistry;

    private readonly List<string> registeredKeys = new();
    private int wagonCounter;

    private Vector3 tailPos;
    private Quaternion tailRot;

    // Ancla fija del frente del display: acá spawnea siempre el próximo wagon comprado
    private Vector3 headPos;
    private Quaternion headRot;

    public Dictionary<int, ShopWagonData> InstantiatedWagonReferences => instantiatedWagonReferences;

    private void Awake()
    {
        cinematicActorRegistry = ServiceLocator.Get<ICinematicActorRegistry>();

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

        headPos = tailPos;
        headRot = tailRot;

        int counter = 0;
        foreach (var wagon in wagonList)
        {
            instantiatedWagonReferences.Add(counter, CreateWagon(wagonAssetsReference[wagon.WagonName], wagon).Item2);
            Debug.Log("Display train: " + wagonAssetsReference[wagon.WagonName]);

            counter++;
        }

        ServiceLocator.Register(this);
    }

    private void OnDestroy()
    {
        foreach (var key in registeredKeys)
            cinematicActorRegistry?.UnregisterDynamic(key);

        registeredKeys.Clear();
    }

    #region create and add wagons
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

        GameObject newWagon = Instantiate(wagonID.shopModel, headPos, headRot);
        ShopWagonData newWagonData = newWagon.GetComponent<ShopWagonData>();
        newWagonData.SetID(newWag);

        // Cuánto espacio ocupa el wagon nuevo (mismo criterio que CreateWagon: hasta su propio socket "tail")
        Transform newWagonTail = newWagonData.tail;
        Vector3 shiftOffset = newWagonTail.position - headPos;

        // Corre para atrás todos los wagons ya instanciados, para hacerle lugar al nuevo adelante
        foreach (var wagon in instantiatedWagonReferences.Values)
        {
            Vector3 targetPos = wagon.transform.position + shiftOffset;
            wagon.transform.DOMove(targetPos, shiftDuration).SetEase(shiftEase);
        }

        // Reindexa: todo lo existente corre +1, el nuevo ocupa el slot 0 (el frente)
        var reindexed = new Dictionary<int, ShopWagonData> { [0] = newWagonData };
        foreach (var kvp in instantiatedWagonReferences)
            reindexed[kvp.Key + 1] = kvp.Value;
        instantiatedWagonReferences = reindexed;

        wagonList.AddFirst(newWag);

        // El fondo del tren también se corre para atrás
        tailPos += shiftOffset;

        // Pop-in: arranca en 0 y escala hasta su tamaño real
        Vector3 finalScale = newWagon.transform.localScale;
        newWagon.transform.localScale = Vector3.zero;
        newWagon.transform.DOScale(finalScale, popInDuration).SetEase(popInEase).SetDelay(popInDelay);

        string key = $"shop_wagon_{wagonCounter++}";
        registeredKeys.Add(key);

        cinematicActorRegistry.RegisterDynamic(key, newWagon.transform);
        EventBus.Publish(new OnWagonAddedToDisplayEvent(key));

        return newWagon;
    }

    #endregion

    public void ReorderWagons(ShopWagonData selected, ShopWagonData objective, int selectedKey, int objectiveKey)
    {
        Vector3 reference = selected.transform.position;

        selected.transform.position = objective.transform.position;
        SetLayerRecursively(selected.gameObject, LayerMask.NameToLayer("Outline"));

        objective.transform.position = reference;
        SetLayerRecursively(objective.gameObject, LayerMask.NameToLayer("Outline"));

        var wagonA = wagonList.Find(selected.IDReference);
        var wagonB = wagonList.Find(objective.IDReference);

        instantiatedWagonReferences[selectedKey] = objective;
        instantiatedWagonReferences[objectiveKey] = selected;

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
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        foreach (Transform t in obj.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.layer = layer;
        }
    }
}