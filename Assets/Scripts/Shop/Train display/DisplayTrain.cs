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

    [Header("Upgrades")]
    [SerializeField] private List<WagonUpgradePathSO> upgradePaths;
    [SerializeField] private float upgradePunchScale = 0.15f;
    [SerializeField] private float upgradePunchDuration = 0.35f;

    private Dictionary<string, (WagonUpgradePathSO path, int level)> upgradeLookup;

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
        upgradeLookup = new Dictionary<string, (WagonUpgradePathSO, int)>();

        foreach (var asset in wagonAssets)
        {
            if (asset == null) continue;
            wagonAssetsReference[asset.wagonName] = asset.shopModel;
        }

        if (upgradePaths == null || upgradePaths.Count == 0)
            Debug.LogWarning("[DisplayTrain] 'upgradePaths' está vacío: ningún vagón va a poder mejorarse.", this);
        else
        {
            foreach (var path in upgradePaths)
            {
                if (path == null || path.levels == null) continue;

                for (int i = 0; i < path.levels.Length; i++)
                {
                    var so = path.levels[i].wagon;
                    if (so == null) continue;

                    // Todos los niveles entran al lookup: sin esto, un vagón de nivel 2
                    // no encuentra su shop model al volver de la run.
                    wagonAssetsReference[so.wagonName] = so.shopModel;
                    upgradeLookup[so.wagonName] = (path, i);
                }
            }
        }

        wagonList = new LinkedList<IWagonID>();
        foreach (var wagon in StoreManager.Instance.wagonsInTrain)
            wagonList.AddLast(wagon);

        tailPos = currentTail.position;
        tailRot = currentTail.rotation;

        headPos = tailPos;
        headRot = tailRot;

        int counter = 0;
        foreach (var wagon in wagonList)
        {
            if (!wagonAssetsReference.TryGetValue(wagon.WagonName, out var model) || model == null)
            {
                // Se omite solo del display; sigue en wagonList y vuelve a la run igual.
                Debug.LogError($"[DisplayTrain] '{wagon.WagonName}' no está en wagonAssets ni en upgradePaths. Se omite del display.", this);
                continue;
            }

            instantiatedWagonReferences.Add(counter, CreateWagon(model, wagon).Item2);
            counter++;
        }

        ServiceLocator.Register(this);
    }

    private void OnDestroy()
    {
        foreach (var key in registeredKeys)
            cinematicActorRegistry?.UnregisterDynamic(key);

        registeredKeys.Clear();

        ServiceLocator.Unregister<DisplayTrain>();
    }

    // Lleva cualquier tween en curso (shift, pop-in, punch) a su estado final,
    // para que los cálculos de posición partan de valores definitivos.
    private void CompleteAllTweens()
    {
        if (instantiatedWagonReferences == null) return;

        foreach (var w in instantiatedWagonReferences.Values)
            if (w != null) w.transform.DOComplete();
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
        CompleteAllTweens();

        var newWag = new WagonStore(wagonID.Wagon, wagonID.wagonName, wagonID.Price);

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

        CompleteAllTweens();

        wagonList.Remove(wagonData.IDReference);

        // Mismo paso que usan CreateWagon y ComputeReflowPositions: footprint + gap.
        // Así los vagones de atrás quedan exactamente donde el reflow los espera.
        Vector3 span = wagonData.FootprintOffset - (wagonData.transform.rotation * Vector3.forward) * wagonGap;

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

        wagonData.transform.DOKill();
        Destroy(wagonData.gameObject);

        return wagonData;
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
        if (instantiatedWagonReferences == null || instantiatedWagonReferences.Count == 0) return;

        // Sin esto, si el slot 0 se está moviendo (venta reciente) el ancla queda a mitad de camino
        // y todo el tren se desplaza; y un pop-in a medias podría quedar cortado.
        CompleteAllTweens();

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

    #region upgrade wagon

    // Nivel actual (1..N) del wagon en el slot, o 0 si no pertenece a ningún path.
    public int GetWagonLevel(int slotIndex)
    {
        if (!instantiatedWagonReferences.TryGetValue(slotIndex, out var w)) return 0;
        return upgradeLookup.TryGetValue(w.IDReference.WagonName, out var e) ? e.level + 1 : 0;
    }

    // El costo de mejora es el Price del SO del siguiente nivel.
    public bool TryGetUpgradeInfo(int slotIndex, out WagonInStockSO next, out float cost)
    {
        next = null;
        cost = 0f;

        if (!instantiatedWagonReferences.TryGetValue(slotIndex, out var wagon)) return false;
        if (!upgradeLookup.TryGetValue(wagon.IDReference.WagonName, out var entry)) return false;

        int nextIndex = entry.level + 1;
        if (nextIndex >= entry.path.levels.Length) return false; // nivel máximo

        next = entry.path.levels[nextIndex].wagon;
        if (next == null) return false;

        cost = next.Price;
        return true;
    }

    // Reemplaza el wagon del slot por su siguiente nivel y reacomoda el tren.
    // No cobra: el caller (ReorderManager) se encarga del oro.
    public ShopWagonData UpgradeWagon(int slotIndex)
    {
        if (!TryGetUpgradeInfo(slotIndex, out var next, out _)) return null;

        // Completar tweens antes de leer la posición del wagon viejo
        CompleteAllTweens();

        var oldWagon = instantiatedWagonReferences[slotIndex];

        // 1. Datos lógicos: mismo nodo en la lista, el orden no cambia.
        //    Price = precio del nivel actual → la venta devuelve la fracción de la última mejora.
        var newID = new WagonStore(next.Wagon, next.wagonName, next.Price);
        var node = wagonList.Find(oldWagon.IDReference);
        if (node != null) node.Value = newID;

        // 2. Modelo nuevo en el mismo lugar
        GameObject newGO = Instantiate(next.shopModel, oldWagon.transform.position, oldWagon.transform.rotation);
        newGO.layer = oldWagon.gameObject.layer;

        ShopWagonData newWagon = newGO.GetComponent<ShopWagonData>();
        newWagon.SetID(newID);

        // 3. Cinematic key: se conserva, apuntando al transform nuevo
        if (!string.IsNullOrEmpty(oldWagon.CinematicKey))
        {
            cinematicActorRegistry?.UnregisterDynamic(oldWagon.CinematicKey);
            cinematicActorRegistry?.RegisterDynamic(oldWagon.CinematicKey, newGO.transform);
            newWagon.CinematicKey = oldWagon.CinematicKey;
        }

        instantiatedWagonReferences[slotIndex] = newWagon;
        oldWagon.transform.DOKill();
        Destroy(oldWagon.gameObject);

        // 4. Reflow completo con el footprint nuevo
        CacheSlotLayout();
        var positions = ComputeReflowPositions();

        foreach (var kvp in instantiatedWagonReferences)
        {
            if (kvp.Key == slotIndex)
            {
                kvp.Value.transform.position = positions[kvp.Key];
                continue;
            }

            kvp.Value.transform.DOMove(positions[kvp.Key], shiftDuration).SetEase(shiftEase);
            kvp.Value.transform.DORotateQuaternion(reflowRot, shiftDuration);
        }

        // 5. tailPos = cola del último wagon en su posición final
        int lastIndex = instantiatedWagonReferences.Count - 1;
        tailPos = positions[lastIndex] + instantiatedWagonReferences[lastIndex].FootprintOffset;

        newGO.transform.DOPunchScale(Vector3.one * upgradePunchScale, upgradePunchDuration, 6, 0.5f);

        return newWagon;
    }

    #endregion

    public List<IWagonID> ChangeWagonIDList()
    {
        return wagonList.ToList();
    }
}