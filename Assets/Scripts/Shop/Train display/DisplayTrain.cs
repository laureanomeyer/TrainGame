using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class DisplayTrain : MonoBehaviour
{
    [SerializeField] private Transform currentTail;

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
    private Dictionary<int, ShopWagonData> instantiatedWagonReferences;

    private ICinematicActorRegistry cinematicActorRegistry;
    private readonly List<string> registeredKeys = new();
    private int wagonCounter;


    private Vector3 layoutOrigin;     
    private Quaternion layoutRot;        
    private Vector3 LayoutForward => layoutRot * Vector3.forward;


    private readonly Dictionary<Transform, Tween> moveTweens = new();
    private readonly Dictionary<Transform, Tween> rotTweens = new();

    private int draggedSlot = -1;
    private int dragOriginSlot = -1;
    private ShopWagonData draggedWagon;

    public Dictionary<int, ShopWagonData> InstantiatedWagonReferences => instantiatedWagonReferences;
    public int DraggedSlot => draggedSlot;
    public ShopWagonData DraggedWagon => draggedWagon;

    #region Init

    public void Initialize(List<IWagonID> wagonsInTrain)
    {
        cinematicActorRegistry = ServiceLocator.Get<ICinematicActorRegistry>();

        wagonAssetsReference = new Dictionary<string, GameObject>();
        upgradeLookup = new Dictionary<string, (WagonUpgradePathSO, int)>();
        soLookup = new Dictionary<string, WagonInStockSO>();
        instantiatedWagonReferences = new Dictionary<int, ShopWagonData>();

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

        wagonList = new LinkedList<IWagonID>();
        foreach (var wagon in wagonsInTrain)
            wagonList.AddLast(wagon);

        int slot = 0;
        foreach (var wagon in wagonList)
        {
            if (!wagonAssetsReference.TryGetValue(wagon.WagonName, out var model) || model == null)
            {
                
                Debug.LogError($"[DisplayTrain] '{wagon.WagonName}' no está en wagonAssets ni en upgradePaths. Se omite del display.", this);
                continue;
            }

            instantiatedWagonReferences[slot++] = SpawnWagon(model, wagon);
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

    private Vector3[] ComputeLayout()
    {
        int count = instantiatedWagonReferences.Count;
        var positions = new Vector3[count];
        Vector3 cursor = layoutOrigin;

        for (int i = 0; i < count; i++)
        {
            positions[i] = cursor;
            cursor += StepOf(instantiatedWagonReferences[i]);
        }

        return positions;
    }


    private void ApplyLayout(float duration, Ease ease, int liftedSlot = -1, ShopWagonData snapWagon = null)
    {
        var positions = ComputeLayout();

        foreach (var kvp in instantiatedWagonReferences)
        {
            Vector3 target = positions[kvp.Key];
            if (kvp.Key == liftedSlot) target += Vector3.up * dragLiftHeight;

            bool snap = duration <= 0f || kvp.Value == snapWagon;
            MoveWagon(kvp.Value.transform, target, snap ? 0f : duration, ease);
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

        moveTweens[t] = t.DOMove(pos, duration).SetEase(ease);
        rotTweens[t] = t.DORotateQuaternion(layoutRot, duration).SetEase(ease);
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


        var reindexed = new Dictionary<int, ShopWagonData> { [0] = newWagonData };
        foreach (var kvp in instantiatedWagonReferences)
            reindexed[kvp.Key + 1] = kvp.Value;
        instantiatedWagonReferences = reindexed;

        wagonList.AddFirst(newWag);

        ApplyLayout(shiftDuration, shiftEase, snapWagon: newWagonData);

        var newWagon = newWagonData.gameObject;
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

    #region Sell

    public ShopWagonData SellWagon(int slotIndex)
    {
        if (!instantiatedWagonReferences.TryGetValue(slotIndex, out var wagonData)) return null;

        wagonList.Remove(wagonData.IDReference);

        if (!string.IsNullOrEmpty(wagonData.CinematicKey))
        {
            cinematicActorRegistry?.UnregisterDynamic(wagonData.CinematicKey);
            registeredKeys.Remove(wagonData.CinematicKey);
        }

        var reindexed = new Dictionary<int, ShopWagonData>();
        foreach (var kvp in instantiatedWagonReferences)
        {
            if (kvp.Key == slotIndex) continue;
            reindexed[kvp.Key > slotIndex ? kvp.Key - 1 : kvp.Key] = kvp.Value;
        }
        instantiatedWagonReferences = reindexed;

        DestroyWagon(wagonData);
        ApplyLayout(shiftDuration, shiftEase);

        return wagonData; 
    }

    #endregion

    #region Drag reorder

    public void BeginDrag(int slotIndex)
    {
        if (!instantiatedWagonReferences.TryGetValue(slotIndex, out var wagon)) return;

        draggedSlot = slotIndex;
        dragOriginSlot = slotIndex;
        draggedWagon = wagon;

        ApplyLayout(dragMoveDuration, dragMoveEase, liftedSlot: draggedSlot);
    }

    public bool StepDrag(int direction)
    {
        if (draggedWagon == null) return false;

        int targetSlot = draggedSlot - direction;
        if (targetSlot < 0 || targetSlot >= instantiatedWagonReferences.Count) return false;

        instantiatedWagonReferences[draggedSlot] = instantiatedWagonReferences[targetSlot];
        instantiatedWagonReferences[targetSlot] = draggedWagon;
        draggedSlot = targetSlot;

        ApplyLayout(dragMoveDuration, dragMoveEase, liftedSlot: draggedSlot);
        return true;
    }

    public void EndDrag()
    {
        if (draggedWagon == null) return;

        ApplyLayout(dragMoveDuration, dragMoveEase);
        SyncWagonListFromSlots();

        draggedWagon = null;
        draggedSlot = -1;
        dragOriginSlot = -1;
    }

    public int CancelDrag()
    {
        if (draggedWagon == null) return draggedSlot;

        int origin = dragOriginSlot;

        var ordered = instantiatedWagonReferences.OrderBy(k => k.Key).Select(k => k.Value).ToList();
        ordered.Remove(draggedWagon);
        ordered.Insert(origin, draggedWagon);

        instantiatedWagonReferences = new Dictionary<int, ShopWagonData>();
        for (int i = 0; i < ordered.Count; i++)
            instantiatedWagonReferences[i] = ordered[i];

        ApplyLayout(dragMoveDuration, dragMoveEase);

        draggedWagon = null;
        draggedSlot = -1;
        dragOriginSlot = -1;

        return origin;
    }

    private void SyncWagonListFromSlots()
    {
        wagonList.Clear();
        foreach (var kvp in instantiatedWagonReferences.OrderBy(k => k.Key))
            wagonList.AddLast(kvp.Value.IDReference);
    }

    #endregion

    #region Upgrade

    public int GetWagonLevel(int slotIndex)
    {
        if (!instantiatedWagonReferences.TryGetValue(slotIndex, out var w)) return 0;
        return upgradeLookup.TryGetValue(w.IDReference.WagonName, out var e) ? e.level + 1 : 0;
    }

    public WagonInStockSO GetWagonSO(int slotIndex)
    {
        if (!instantiatedWagonReferences.TryGetValue(slotIndex, out var w)) return null;
        return soLookup.TryGetValue(w.IDReference.WagonName, out var so) ? so : null;
    }

    public bool TryGetUpgradeInfo(int slotIndex, out WagonInStockSO next, out float cost)
    {
        next = null;
        cost = 0f;

        if (!instantiatedWagonReferences.TryGetValue(slotIndex, out var wagon)) return false;
        if (!upgradeLookup.TryGetValue(wagon.IDReference.WagonName, out var entry)) return false;

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

        var oldWagon = instantiatedWagonReferences[slotIndex];

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

        instantiatedWagonReferences[slotIndex] = newWagon;
        DestroyWagon(oldWagon);

        ApplyLayout(shiftDuration, shiftEase, snapWagon: newWagon);

        newWagon.transform.DOPunchScale(Vector3.one * upgradePunchScale, upgradePunchDuration, 6, 0.5f);

        return newWagon;
    }

    #endregion

    public List<IWagonID> ChangeWagonIDList() => wagonList.ToList();
}