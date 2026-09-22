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
        var newWag = new WagonStore(wagonID.Wagon, wagonID.wagonName, wagonID.Price, wagonID.LevelSet, 1);

        Quaternion rotation = new Quaternion(0.00000f, -0.70711f, 0.00000f, 0.70711f);

        GameObject newWagon = Instantiate(wagonID.shopModel, headPos, rotation);
        ShopWagonData newWagonData = newWagon.GetComponent<ShopWagonData>();
        newWagonData.SetID(newWag);

        Transform newWagonTail = newWagonData.tail;
        Vector3 shiftOffset = newWagonTail.position - headPos;

        foreach (var wagon in instantiatedWagonReferences.Values)
        {
            Vector3 targetPos = wagon.transform.position + shiftOffset;
            wagon.transform.DOMove(targetPos, shiftDuration).SetEase(shiftEase);
        }

        var reindexed = new Dictionary<int, ShopWagonData> { [0] = newWagonData };
        foreach (var kvp in instantiatedWagonReferences)
            reindexed[kvp.Key + 1] = kvp.Value;
        instantiatedWagonReferences = reindexed;

        wagonList.AddFirst(newWag);

        tailPos += shiftOffset;

        Vector3 finalScale = newWagon.transform.localScale;
        newWagon.transform.localScale = Vector3.zero;
        newWagon.transform.DOScale(finalScale, popInDuration).SetEase(popInEase).SetDelay(popInDelay);

        string key = $"shop_wagon_{wagonCounter++}";
        registeredKeys.Add(key);
        newWagonData.CinematicKey = key;

        cinematicActorRegistry.RegisterDynamic(key, newWagon.transform);
        EventBus.Publish(new OnWagonAddedToDisplayEvent(key));

        return newWagon;
    }

    #endregion

    #region sell wagon

    public ShopWagonData SellWagon(int slotIndex)
    {
        if (!instantiatedWagonReferences.TryGetValue(slotIndex, out var wagonData)) return null;

        wagonList.Remove(wagonData.IDReference);

        Vector3 span = wagonData.FootprintOffset;

        if (!string.IsNullOrEmpty(wagonData.CinematicKey))
        {
            cinematicActorRegistry?.UnregisterDynamic(wagonData.CinematicKey);
            registeredKeys.Remove(wagonData.CinematicKey);
        }

        var reindexed = new Dictionary<int, ShopWagonData>();
        foreach (var kvp in instantiatedWagonReferences)
        {
            if (kvp.Key == slotIndex) continue;

            if (kvp.Key > slotIndex)
            {
                Vector3 targetPos = kvp.Value.transform.position - span;
                kvp.Value.transform.DOMove(targetPos, shiftDuration).SetEase(shiftEase);
                reindexed[kvp.Key - 1] = kvp.Value;
            }
            else
            {
                reindexed[kvp.Key] = kvp.Value;
            }
        }
        instantiatedWagonReferences = reindexed;

        tailPos -= span;

        Destroy(wagonData.gameObject);

        return wagonData;
    }

    #endregion

    #region upgrade wagon

    // Devuelve el costo para subir de nivel el wagon en ese slot, o null si no puede
    // mejorarse (sin LevelSet asignado, o ya en el nivel máximo definido).
    public float? GetUpgradeCost(int slotIndex)
    {
        if (!instantiatedWagonReferences.TryGetValue(slotIndex, out var wagonData)) return null;

        var levelSet = wagonData.IDReference?.LevelSet;
        if (levelSet == null) return null;

        var entry = levelSet.GetLevel(wagonData.IDReference.Level + 1);
        return entry?.upgradeCost;
    }

    // Reemplaza el prefab del wagon en ese slot por el de su próximo nivel, y reacomoda
    // el resto del tren si el nuevo modelo ocupa distinto espacio. No cobra el oro:
    // eso lo maneja el caller (ver ReorderManager) antes de llamar a este método.
    public bool UpgradeWagon(int slotIndex)
    {
        if (!instantiatedWagonReferences.TryGetValue(slotIndex, out var oldWagonData)) return false;

        IWagonID id = oldWagonData.IDReference;
        var levelSet = id?.LevelSet;
        if (levelSet == null) return false;

        int nextLevel = id.Level + 1;
        var entry = levelSet.GetLevel(nextLevel);
        if (entry == null || entry.shopModel == null || entry.gameplayPrefab == null) return false;

        Vector3 oldFootprint = oldWagonData.FootprintOffset;
        Vector3 spawnPos = oldWagonData.transform.position;
        Quaternion spawnRot = oldWagonData.transform.rotation;
        string cinematicKey = oldWagonData.CinematicKey;

        // Instancia el nuevo modelo en el mismo lugar exacto que ocupaba el anterior
        GameObject newWagonObj = Instantiate(entry.shopModel, spawnPos, spawnRot);
        ShopWagonData newWagonData = newWagonObj.GetComponent<ShopWagonData>();

        id.Level = nextLevel;
        id.Prefab = entry.gameplayPrefab; // prefab de gameplay que usará TrainManager en combate
        id.Price += entry.upgradeCost;    // acumula inversión para que el reembolso al vender la contemple

        newWagonData.SetID(id);
        newWagonData.CinematicKey = cinematicKey;

        if (!string.IsNullOrEmpty(cinematicKey))
        {
            cinematicActorRegistry?.UnregisterDynamic(cinematicKey);
            cinematicActorRegistry?.RegisterDynamic(cinematicKey, newWagonObj.transform);
        }

        Destroy(oldWagonData.gameObject);
        instantiatedWagonReferences[slotIndex] = newWagonData;

        // Si el nuevo modelo ocupa distinto espacio (nivel 2 duplica tamaño), corremos
        // todo lo que está detrás para que no se pise, mismo criterio que Sell/Add.
        Vector3 newFootprint = newWagonData.FootprintOffset;
        Vector3 delta = newFootprint - oldFootprint;

        if (delta != Vector3.zero)
        {
            foreach (var kvp in instantiatedWagonReferences)
            {
                if (kvp.Key <= slotIndex) continue;

                Vector3 targetPos = kvp.Value.transform.position + delta;
                kvp.Value.transform.DOMove(targetPos, shiftDuration).SetEase(shiftEase);
            }

            tailPos += delta;
        }

        // Pop visual del upgrade, mismo lenguaje que el pop-in de compra
        Vector3 finalScale = newWagonObj.transform.localScale;
        newWagonObj.transform.localScale = finalScale * 0.01f;
        newWagonObj.transform.DOScale(finalScale, popInDuration).SetEase(popInEase);

        return true;
    }

    #endregion

    #region drag reorder

    [Header("Drag Reorder")]
    [SerializeField] private float dragLiftHeight = 5f;
    [SerializeField] private float dragMoveDuration = 0.3f;
    [SerializeField] private Ease dragMoveEase = Ease.OutQuad;

    private Vector3 reflowAnchorPos;
    private Quaternion reflowRot;

    private int draggedSlot = -1;
    private ShopWagonData draggedWagon;

    public int DraggedSlot => draggedSlot;
    public ShopWagonData DraggedWagon => draggedWagon;

    public void CacheSlotLayout()
    {
        var frontWagon = instantiatedWagonReferences[0];
        reflowAnchorPos = frontWagon.transform.position;
        reflowRot = frontWagon.transform.rotation;
    }

    private Dictionary<int, Vector3> ComputeReflowPositions()
    {
        var positions = new Dictionary<int, Vector3>();
        Vector3 cursor = reflowAnchorPos;

        for (int i = 0; i < instantiatedWagonReferences.Count; i++)
        {
            positions[i] = cursor;

            Vector3 footprint = instantiatedWagonReferences[i].FootprintOffset;
            cursor += footprint - (reflowRot * Vector3.forward) * wagonGap;
        }

        return positions;
    }

    public void BeginDrag(int slotIndex)
    {
        if (!instantiatedWagonReferences.TryGetValue(slotIndex, out var wagon)) return;

        draggedSlot = slotIndex;
        draggedWagon = wagon;

        var positions = ComputeReflowPositions();
        Vector3 liftedPos = positions[slotIndex] + Vector3.up * dragLiftHeight;
        wagon.transform.DOMove(liftedPos, dragMoveDuration).SetEase(dragMoveEase);
    }

    public bool StepDrag(int direction)
    {
        if (draggedWagon == null) return false;

        int targetSlot = draggedSlot - direction;
        if (targetSlot < 0 || targetSlot >= instantiatedWagonReferences.Count) return false;

        var otherWagon = instantiatedWagonReferences[targetSlot];

        instantiatedWagonReferences[draggedSlot] = otherWagon;
        instantiatedWagonReferences[targetSlot] = draggedWagon;

        draggedSlot = targetSlot;

        var positions = ComputeReflowPositions();

        foreach (var kvp in instantiatedWagonReferences)
        {
            bool isDragged = kvp.Key == draggedSlot;
            Vector3 targetPos = positions[kvp.Key] + (isDragged ? Vector3.up * dragLiftHeight : Vector3.zero);

            kvp.Value.transform.DOMove(targetPos, dragMoveDuration).SetEase(dragMoveEase);
            if (!isDragged) kvp.Value.transform.DORotateQuaternion(reflowRot, dragMoveDuration);
        }

        return true;
    }

    public void EndDrag()
    {
        if (draggedWagon == null) return;

        var positions = ComputeReflowPositions();
        Vector3 finalPos = positions[draggedSlot];

        draggedWagon.transform.DOMove(finalPos, dragMoveDuration).SetEase(dragMoveEase);
        draggedWagon.transform.DORotateQuaternion(reflowRot, dragMoveDuration);

        SyncWagonListFromSlots();

        draggedWagon = null;
        draggedSlot = -1;
    }

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