using UnityEngine;
using UnityEngine.InputSystem;

public class ReorderManager : MonoBehaviour
{
    [SerializeField] private PlayerInput inputRef;
    [SerializeField] private float sellRefundFraction = 0.5f;

    [Header("Option descriptions")]
    [SerializeField] private string moveDescription = "Change wagon order";
    [SerializeField] private string maxLevelDescription = "Max level";

    private DisplayTrain trainDisplayRef;
    private StoreUiInteracts UIRef;
    private ReorderCameraController reorderCameraRef;
    private ReorderHUD hudRef;

    private enum State { Hovering, Panel, Dragging }
    private State state = State.Hovering;

    private ShopWagonData cacheRef;
    private int hoveredSlot = -1;
    private ReorderHUD.Option selectedOption = ReorderHUD.Option.Move;
    private bool isInReorderMode;

    private const int OptionCount = 3;

    #region Lifecycle

    private void OnEnable()
    {
        ServiceLocator.Register(this);

        inputRef.actions["Move"].performed += OnMovePerformed;
        inputRef.actions["Jump"].performed += OnSelectPerformed;
        inputRef.actions["Pause"].performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        ServiceLocator.Unregister<ReorderManager>();

        if (inputRef == null) return;
        inputRef.actions["Move"].performed -= OnMovePerformed;
        inputRef.actions["Jump"].performed -= OnSelectPerformed;
        inputRef.actions["Pause"].performed -= OnPausePerformed;
    }

    private void ResolveRefs()
    {
        if (trainDisplayRef == null) ServiceLocator.TryGet(out trainDisplayRef);
        if (UIRef == null) ServiceLocator.TryGet(out UIRef);
        if (reorderCameraRef == null) ServiceLocator.TryGet(out reorderCameraRef);
        if (hudRef == null) ServiceLocator.TryGet(out hudRef);
    }

    #endregion

    #region Input

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        if (!isInReorderMode) return;

        int dir = Mathf.RoundToInt(ctx.ReadValue<Vector2>().x);
        if (dir == 0) return;

        ResolveRefs();

        switch (state)
        {
            case State.Hovering:
                MoveHover(dir);
                break;

            case State.Panel:
                selectedOption = (ReorderHUD.Option)(((int)selectedOption + dir + OptionCount) % OptionCount);
                RefreshPanel();
                break;

            case State.Dragging:
                if (trainDisplayRef.StepDrag(dir))
                {
                    hoveredSlot = trainDisplayRef.DraggedSlot;
                    reorderCameraRef?.SetTarget(trainDisplayRef.DraggedWagon.transform);
                }
                break;
        }
    }

    private void OnSelectPerformed(InputAction.CallbackContext ctx)
    {
        if (!isInReorderMode) return;
        ResolveRefs();

        switch (state)
        {
            case State.Hovering:
                if (cacheRef == null) return;
                state = State.Panel;
                selectedOption = ReorderHUD.Option.Move;
                RefreshPanel();
                break;

            case State.Panel:
                ExecuteSelectedOption();
                break;

            case State.Dragging:
                trainDisplayRef.EndDrag();
                state = State.Hovering;
                ShowHover();
                break;
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (!isInReorderMode) return;
        ResolveRefs();

        switch (state)
        {
            case State.Hovering:
                ToggleReorderMode(false);
                break;

            case State.Panel:
                state = State.Hovering;
                ShowHover();
                break;

            case State.Dragging:
                hoveredSlot = trainDisplayRef.CancelDrag();
                reorderCameraRef?.SetTarget(cacheRef.transform);
                state = State.Hovering;
                ShowHover();
                break;
        }
    }

    #endregion

    #region Hover

    private void MoveHover(int dir)
    {
        int count = trainDisplayRef.InstantiatedWagonReferences.Count;
        if (count == 0) return;

        int slot = hoveredSlot - dir;
        if (slot > count - 1) slot = 0;
        if (slot < 0) slot = count - 1;

        SetHovered(slot, moveCamera: true);
        ShowHover();
    }

    private void SetHovered(int slot, bool moveCamera)
    {
        if (cacheRef != null) SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("Outline"));

        hoveredSlot = slot;
        cacheRef = trainDisplayRef.InstantiatedWagonReferences[slot];
        SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("WhiteOutline"));

        if (moveCamera) reorderCameraRef?.SetTarget(cacheRef.transform);
    }

    private void ShowHover()
    {
        if (hudRef == null || cacheRef == null) return;

        var so = trainDisplayRef.GetWagonSO(hoveredSlot);
        string wagonName = so != null ? so.wagonName : cacheRef.IDReference.WagonName;
        string description = so != null ? so.Description : string.Empty;

        hudRef.ShowHover(cacheRef, wagonName, description);
    }

    #endregion

    #region Panel

    private void RefreshPanel()
    {
        if (hudRef == null || cacheRef == null) return;

        var so = trainDisplayRef.GetWagonSO(hoveredSlot);
        string wagonName = so != null ? so.wagonName : cacheRef.IDReference.WagonName;

        bool hasUpgrade = trainDisplayRef.TryGetUpgradeInfo(hoveredSlot, out _, out float cost);
        bool canAfford = hasUpgrade && StoreManager.Instance.GetGold() >= cost;

        string description = selectedOption switch
        {
            ReorderHUD.Option.Move => moveDescription,
            ReorderHUD.Option.Upgrade => hasUpgrade ? $"Upgrade for {hudRef.FormatMoney(cost, canAfford)}" : maxLevelDescription,
            _ => $"Sell for {hudRef.FormatMoney(GetSellValue(cacheRef), true)}"
        };

        hudRef.ShowPanel(cacheRef, wagonName, selectedOption, description, canAfford);
    }

    private void ExecuteSelectedOption()
    {
        switch (selectedOption)
        {
            case ReorderHUD.Option.Move:
                trainDisplayRef.CacheSlotLayout();
                trainDisplayRef.BeginDrag(hoveredSlot);
                state = State.Dragging;
                hudRef?.ShowDrag(cacheRef);
                break;

            case ReorderHUD.Option.Upgrade:
                TryUpgradeHovered();
                break;

            case ReorderHUD.Option.Sell:
                SellHovered();
                break;
        }
    }

    private void TryUpgradeHovered()
    {
        if (!trainDisplayRef.TryGetUpgradeInfo(hoveredSlot, out _, out float cost)) return;
        if (!StoreManager.Instance.TrySpendGold(cost)) return;

        var upgraded = trainDisplayRef.UpgradeWagon(hoveredSlot);
        if (upgraded == null)
        {
            StoreManager.Instance.AddGold(cost); // devolución por seguridad
            return;
        }

        cacheRef = null; // el modelo viejo fue destruido
        SetHovered(hoveredSlot, moveCamera: true);
        state = State.Hovering;
        ShowHover();
    }

    private void SellHovered()
    {
        var sold = trainDisplayRef.SellWagon(hoveredSlot);
        if (sold == null) return;

        StoreManager.Instance.AddGold(GetSellValue(sold));
        cacheRef = null;

        int count = trainDisplayRef.InstantiatedWagonReferences.Count;
        if (count == 0)
        {
            hoveredSlot = -1;
            ToggleReorderMode(false);
            return;
        }

        SetHovered(Mathf.Min(hoveredSlot, count - 1), moveCamera: true);
        state = State.Hovering;
        ShowHover();
    }

    private float GetSellValue(ShopWagonData wagon) =>
        Mathf.Floor(wagon.IDReference.Price * sellRefundFraction);

    #endregion

    #region Mode toggle

    public void ToggleReorderMode(bool toggled)
    {
        ResolveRefs();
        if (trainDisplayRef == null) return;
        if (toggled == isInReorderMode) return;

        if (toggled)
        {
            int count = trainDisplayRef.InstantiatedWagonReferences.Count;
            if (count <= 0) return;

            state = State.Hovering;
            trainDisplayRef.CacheSlotLayout();

            int slot = (hoveredSlot < 0 || hoveredSlot >= count) ? 0 : hoveredSlot;
            SetHovered(slot, moveCamera: false);
            reorderCameraRef?.Activate(cacheRef.transform);

            UIRef?.HideUI();
            EventBus.Publish(new OnShowCursorEvent(CursorType.HiddenAndFrozen));

            isInReorderMode = true;
            ShowHover();
        }
        else
        {
            if (state == State.Dragging) trainDisplayRef.CancelDrag();
            if (cacheRef != null) SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("Outline"));

            state = State.Hovering;
            hudRef?.Hide();
            reorderCameraRef?.Deactivate();
            UIRef?.DeactivateUI();

            isInReorderMode = false;
        }

        EventBus.Publish(new OnActivateUiEvent(!toggled));
    }

    #endregion

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        foreach (Transform t in obj.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }
}