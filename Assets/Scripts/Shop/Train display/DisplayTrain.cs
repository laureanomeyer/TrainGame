using DG.Tweening;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;


public class DisplayTrain : MonoBehaviour
{
    [SerializeField] private Transform currentTail;

    [Header("Spawning Floor")]
    [SerializeField] private GameObject floorTile;
    [SerializeField] private Transform lastEndTileConector;
    [SerializeField] private int numOfWagonsPerTile = 4;
    private List<GameObject> floorTiles;
    private int numberOfTiles = 0;

    [Header("Wagon Spacing")]
    [SerializeField] private float wagonGap = -0.6f;

    [Header("New Wagon Pop-In Animation")]
    [SerializeField] private float popInDelay = 0.5f;
    [SerializeField] private float popInDuration = 0.4f;
    [SerializeField] private Ease popInEase = Ease.OutBack;

    [Header("Existing Wagons Shift Animation")]
    [SerializeField] private float shiftDuration = 0.4f;
    [SerializeField] private Ease shiftEase = Ease.OutQuad;

    [Header("Drag Reorder")]
    [SerializeField] private float dragLiftHeight = 5f;
    [SerializeField] private float dragMoveDuration = 0.3f;
    [SerializeField] private Ease dragMoveEase = Ease.OutQuad;

    [Header("Upgrades")]
    [SerializeField] private List<WagonUpgradePathSO> upgradePaths;
    [SerializeField] private float upgradePunchScale = 0.15f;
    [SerializeField] private float upgradePunchDuration = 0.35f;

    [SerializeField] private List<WagonInStockSO> wagonAssets;

    private Dictionary<string, GameObject> wagonAssetsReference;
    private Dictionary<string, (WagonUpgradePathSO path, int level)> upgradeLookup;
    private Dictionary<string, WagonInStockSO> soLookup;

    private LinkedList<IWagonID> wagonList;

    private readonly List<ShopWagonData> slots = new();

    private ICinematicActorRegistry cinematicActorRegistry;
    private readonly List<string> registeredKeys = new();
    private int wagonCounter;

    private Vector3 layoutOrigin;
    private Quaternion layoutRot;
    private Vector3 LayoutForward => layoutRot * Vector3.forward;

    private readonly List<Vector3> layoutBuffer = new();

    private readonly Dictionary<Transform, Tween> moveTweens = new();
    private readonly Dictionary<Transform, Tween> rotTweens = new();

    private int draggedSlot = -1;
    private int dragOriginSlot = -1;
    private ShopWagonData draggedWagon;

    public IReadOnlyList<ShopWagonData> InstantiatedWagonReferences => slots;
    public int DraggedSlot => draggedSlot;
    public ShopWagonData DraggedWagon => draggedWagon;

    #region Init

    public void Initialize(IEnumerable<IWagonID> wagonsInTrain)
    {
        cinematicActorRegistry = ServiceLocator.Get<ICinematicActorRegistry>();

        wagonAssetsReference = new Dictionary<string, GameObject>();
        upgradeLookup = new Dictionary<string, (WagonUpgradePathSO, int)>();
        soLookup = new Dictionary<string, WagonInStockSO>();
        slots.Clear();

        foreach (var asset in wagonAssets)
        {
            if (asset == null) continue;
            wagonAssetsReference[asset.wagonName] = asset.shopModel;
            soLookup[asset.wagonName] = asset;
        }

        if (upgradePaths == null || upgradePaths.Count == 0)
        {
            Debug.LogWarning("[DisplayTrain] 'upgradePaths' está vacío: ningún vagón va a poder mejorarse.", this);
        }
        else
        {
            foreach (var path in upgradePaths)
            {
                if (path == null || path.levels == null) continue;

                for (int i = 0; i < path.levels.Length; i++)
                {
                    var so = path.levels[i].wagon;
                    if (so == null) continue;

                    wagonAssetsReference[so.wagonName] = so.shopModel;
                    upgradeLookup[so.wagonName] = (path, i);
                    soLookup[so.wagonName] = so;
                }
            }
        }

        layoutRot = currentTail.rotation;
        layoutOrigin = currentTail.position - LayoutForward * wagonGap;

        wagonList = wagonsInTrain != null
            ? new LinkedList<IWagonID>(wagonsInTrain)
            : new LinkedList<IWagonID>();

        foreach (var wagon in wagonList)
        {
            if (!wagonAssetsReference.TryGetValue(wagon.WagonName, out var model) || model == null)
            {
                Debug.LogError($"[DisplayTrain] '{wagon.WagonName}' no está en wagonAssets ni en upgradePaths. Se omite del display.", this);
                continue;
            }

            slots.Add(SpawnWagon(model, wagon));
        }

        ApplyLayout(0f, shiftEase);

        ServiceLocator.Register(this);
    }

    private void OnDestroy()
    {
        foreach (var key in registeredKeys)
            cinematicActorRegistry?.UnregisterDynamic(key);
        registeredKeys.Clear();

        ServiceLocator.Unregister<DisplayTrain>();
    }

    private ShopWagonData SpawnWagon(GameObject model, IWagonID id)
    {
        GameObject go = Instantiate(model, layoutOrigin, layoutRot);
        var data = go.GetComponent<ShopWagonData>();
        data.SetID(id);
        return data;
    }

    #endregion

    #region Layout

    private Vector3 StepOf(ShopWagonData w) => LayoutForward * (w.LocalFootprint.z - wagonGap);

    private void ComputeLayout()
    {
        layoutBuffer.Clear();
        Vector3 cursor = layoutOrigin;

        for (int i = 0; i < slots.Count; i++)
        {
            layoutBuffer.Add(cursor);
            cursor += StepOf(slots[i]);
        }
    }

    private void ApplyLayout(float duration, Ease ease, int liftedSlot = -1, ShopWagonData snapWagon = null)
    {
        ComputeLayout();

        for (int i = 0; i < slots.Count; i++)
        {
            ShopWagonData wagon = slots[i];
            Vector3 target = layoutBuffer[i];
            if (i == liftedSlot) target += Vector3.up * dragLiftHeight;

            bool snap = duration <= 0f || wagon == snapWagon;
            MoveWagon(wagon.transform, target, snap ? 0f : duration, ease);
        }
    }

    private void MoveWagon(Transform t, Vector3 pos, float duration, Ease ease)
    {
        KillLayoutTweens(t);

        if (duration <= 0f)
        {
            t.SetPositionAndRotation(pos, layoutRot);
            return;
        }

        GameObject go = t.gameObject;
        moveTweens[t] = t.DOMove(pos, duration).SetEase(ease).SetLink(go);
        rotTweens[t] = t.DORotateQuaternion(layoutRot, duration).SetEase(ease).SetLink(go);
    }

    private void KillLayoutTweens(Transform t)
    {
        if (moveTweens.TryGetValue(t, out var m) && m.IsActive()) m.Kill();
        if (rotTweens.TryGetValue(t, out var r) && r.IsActive()) r.Kill();
    }

    private void DestroyWagon(ShopWagonData w)
    {
        var t = w.transform;
        KillLayoutTweens(t);
        moveTweens.Remove(t);
        rotTweens.Remove(t);
        t.DOKill();
        Destroy(w.gameObject);
    }

    public void CacheSlotLayout() { }

    #endregion

    #region Add

    public GameObject AddWagon(WagonInStockSO wagonID)
    {
        var newWag = new WagonStore(wagonID.Wagon, wagonID.wagonName, wagonID.Price);
        var newWagonData = SpawnWagon(wagonID.shopModel, newWag);

        slots.Insert(0, newWagonData);
        wagonList.AddFirst(newWag);

        ApplyLayout(shiftDuration, shiftEase, snapWagon: newWagonData);

        var newWagon = newWagonData.gameObject;
        Vector3 finalScale = newWagon.transform.localScale;
        newWagon.transform.localScale = Vector3.zero;
        newWagon.transform.DOScale(finalScale, popInDuration)
            .SetEase(popInEase)
            .SetDelay(popInDelay)
            .SetLink(newWagon);

        string key = "shop_wagon_" + wagonCounter++;
        registeredKeys.Add(key);
        newWagonData.CinematicKey = key;

        cinematicActorRegistry.RegisterDynamic(key, newWagon.transform);
        EventBus.Publish(new OnWagonAddedToDisplayEvent(key));

        return newWagon;
    }

    #endregion

    #region Sell

    public ShopWagonData SellWagon(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) return null;

        var wagonData = slots[slotIndex];
        wagonList.Remove(wagonData.IDReference);

        if (!string.IsNullOrEmpty(wagonData.CinematicKey))
        {
            cinematicActorRegistry?.UnregisterDynamic(wagonData.CinematicKey);
            registeredKeys.Remove(wagonData.CinematicKey);
        }

        slots.RemoveAt(slotIndex);

        DestroyWagon(wagonData);
        ApplyLayout(shiftDuration, shiftEase);

        return wagonData;
    }

    #endregion

    #region Drag reorder

    public void BeginDrag(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) return;

        draggedSlot = slotIndex;
        dragOriginSlot = slotIndex;
        draggedWagon = slots[slotIndex];

        ApplyLayout(dragMoveDuration, dragMoveEase, liftedSlot: draggedSlot);
    }

    public bool StepDrag(int direction)
    {
        if (draggedWagon == null) return false;

        int targetSlot = draggedSlot - direction;
        if (!IsValidSlot(targetSlot)) return false;

        slots[draggedSlot] = slots[targetSlot];
        slots[targetSlot] = draggedWagon;
        draggedSlot = targetSlot;

        ApplyLayout(dragMoveDuration, dragMoveEase, liftedSlot: draggedSlot);
        return true;
    }

    public void EndDrag()
    {
        if (draggedWagon == null) return;

        ApplyLayout(dragMoveDuration, dragMoveEase);
        SyncWagonListFromSlots();

        ClearDragState();
    }

    public int CancelDrag()
    {
        if (draggedWagon == null) return draggedSlot;

        int origin = dragOriginSlot;

        slots.RemoveAt(draggedSlot);
        slots.Insert(Mathf.Clamp(origin, 0, slots.Count), draggedWagon);

        ApplyLayout(dragMoveDuration, dragMoveEase);

        ClearDragState();
        return origin;
    }

    private void ClearDragState()
    {
        draggedWagon = null;
        draggedSlot = -1;
        dragOriginSlot = -1;
    }

    private void SyncWagonListFromSlots()
    {
        wagonList.Clear();
        for (int i = 0; i < slots.Count; i++)
            wagonList.AddLast(slots[i].IDReference);
    }

    private bool IsValidSlot(int slot) => slot >= 0 && slot < slots.Count;

    #endregion

    #region Upgrade

    public int GetWagonLevel(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) return 0;
        return upgradeLookup.TryGetValue(slots[slotIndex].IDReference.WagonName, out var e) ? e.level + 1 : 0;
    }

    public WagonInStockSO GetWagonSO(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) return null;
        return soLookup.TryGetValue(slots[slotIndex].IDReference.WagonName, out var so) ? so : null;
    }

    public bool TryGetUpgradeInfo(int slotIndex, out WagonInStockSO next, out float cost)
    {
        next = null;
        cost = 0f;

        if (!IsValidSlot(slotIndex)) return false;
        if (!upgradeLookup.TryGetValue(slots[slotIndex].IDReference.WagonName, out var entry)) return false;

        int nextIndex = entry.level + 1;
        if (nextIndex >= entry.path.levels.Length) return false;

        next = entry.path.levels[nextIndex].wagon;
        if (next == null) return false;

        cost = next.Price;
        return true;
    }

    public ShopWagonData UpgradeWagon(int slotIndex)
    {
        if (!TryGetUpgradeInfo(slotIndex, out var next, out _)) return null;

        var oldWagon = slots[slotIndex];

        var newID = new WagonStore(next.Wagon, next.wagonName, next.Price);
        var node = wagonList.Find(oldWagon.IDReference);
        if (node != null) node.Value = newID;

        var newWagon = SpawnWagon(next.shopModel, newID);
        newWagon.gameObject.layer = oldWagon.gameObject.layer;

        if (!string.IsNullOrEmpty(oldWagon.CinematicKey))
        {
            cinematicActorRegistry?.UnregisterDynamic(oldWagon.CinematicKey);
            cinematicActorRegistry?.RegisterDynamic(oldWagon.CinematicKey, newWagon.transform);
            newWagon.CinematicKey = oldWagon.CinematicKey;
        }

        slots[slotIndex] = newWagon;
        DestroyWagon(oldWagon);

        ApplyLayout(shiftDuration, shiftEase, snapWagon: newWagon);

        newWagon.transform
            .DOPunchScale(Vector3.one * upgradePunchScale, upgradePunchDuration, 6, 0.5f)
            .SetLink(newWagon.gameObject);

        return newWagon;
    }

    #endregion

    public List<IWagonID> ChangeWagonIDList() => new List<IWagonID>(wagonList);

    private void SpawnTile()
    {
        GameObject spawnTile = Instantiate(floorTile, lastEndTileConector.position, lastEndTileConector.rotation);

        lastEndTileConector = spawnTile.GetComponent<ShopTileData>().endingPoint;

        floorTiles.Add(spawnTile);

        numberOfTiles++;

        Debug.Log("Spawn Tile");
    }

    private void CalculateTilesToSpawn(int numberOfWagons)
    {

    }
}