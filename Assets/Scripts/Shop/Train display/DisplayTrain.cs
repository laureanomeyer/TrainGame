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

    public void Initialize()
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

        //agregar HeadRot en la instanciasion en vez de la varible rotation
        Quaternion rotation = new Quaternion(0.00000f, -0.70711f, 0.00000f, 0.70711f);

        GameObject newWagon = Instantiate(wagonID.shopModel, headPos, rotation);
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

    #region drag reorder

    [Header("Drag Reorder")]
    [SerializeField] private float dragLiftHeight = 5f;
    [SerializeField] private float dragMoveDuration = 0.3f;
    [SerializeField] private Ease dragMoveEase = Ease.OutQuad;

    private Dictionary<int, (Vector3 pos, Quaternion rot)> slotLayout;
    private int draggedSlot = -1;
    private ShopWagonData draggedWagon;

    public int DraggedSlot => draggedSlot;
    public ShopWagonData DraggedWagon => draggedWagon;

    // Guarda la posici�n/rotaci�n "de origen" de cada slot antes de empezar a arrastrar.
    // Estas posiciones quedan fijas durante toda la sesi�n de reorder.
    public void CacheSlotLayout()
    {
        slotLayout = new Dictionary<int, (Vector3, Quaternion)>();
        foreach (var kvp in instantiatedWagonReferences)
            slotLayout[kvp.Key] = (kvp.Value.transform.position, kvp.Value.transform.rotation);
    }

    public void BeginDrag(int slotIndex)
    {
        if (slotLayout == null || !instantiatedWagonReferences.TryGetValue(slotIndex, out var wagon)) return;

        draggedSlot = slotIndex;
        draggedWagon = wagon;

        Vector3 liftedPos = slotLayout[slotIndex].pos + Vector3.up * dragLiftHeight;
        wagon.transform.DOMove(liftedPos, dragMoveDuration).SetEase(dragMoveEase);
    }

    // direction: mismo signo que usa el hover (-1 => avanza hacia atr�s en la lista, +1 => hacia adelante)
    public bool StepDrag(int direction)
    {
        if (draggedWagon == null) return false;

        int targetSlot = draggedSlot - direction;
        if (targetSlot < 0 || targetSlot >= instantiatedWagonReferences.Count) return false;

        var otherWagon = instantiatedWagonReferences[targetSlot];

        // El wagon que estaba en el slot destino cae y ocupa el hueco que dejamos
        Vector3 fillPos = slotLayout[draggedSlot].pos;
        otherWagon.transform.DOMove(fillPos, dragMoveDuration).SetEase(dragMoveEase);
        otherWagon.transform.DORotateQuaternion(slotLayout[draggedSlot].rot, dragMoveDuration);

        // El wagon arrastrado salta (elevado) al slot destino
        Vector3 targetLiftedPos = slotLayout[targetSlot].pos + Vector3.up * dragLiftHeight;
        draggedWagon.transform.DOMove(targetLiftedPos, dragMoveDuration).SetEase(dragMoveEase);

        instantiatedWagonReferences[draggedSlot] = otherWagon;
        instantiatedWagonReferences[targetSlot] = draggedWagon;

        draggedSlot = targetSlot;
        return true;
    }

    public void EndDrag()
    {
        if (draggedWagon == null) return;

        Vector3 finalPos = slotLayout[draggedSlot].pos;
        draggedWagon.transform.DOMove(finalPos, dragMoveDuration).SetEase(dragMoveEase);
        draggedWagon.transform.DORotateQuaternion(slotLayout[draggedSlot].rot, dragMoveDuration);

        SyncWagonListFromSlots();

        draggedWagon = null;
        draggedSlot = -1;
    }

    // Reconstruye el orden l�gico (wagonList) seg�n el orden f�sico final de los slots
    private void SyncWagonListFromSlots()
    {
        wagonList.Clear();
        foreach (var kvp in instantiatedWagonReferences.OrderBy(k => k.Key))
            wagonList.AddLast(kvp.Value.IDReference);
    }

    #endregion

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