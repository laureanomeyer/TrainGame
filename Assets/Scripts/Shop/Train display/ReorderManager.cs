using UnityEngine;
using UnityEngine.InputSystem;

public class ReorderManager : MonoBehaviour
{
    [SerializeField] private StoreManager storeManagerRef;
    [SerializeField] private PlayerInput inputRef;
    [SerializeField] private float sellRefundFraction = 0.5f;

    [Header("Option descriptions")]
    [SerializeField] private string moveDescription = "Change wagon order";
    [SerializeField] private string maxLevelDescription = "Max level";

    [Header("Mouse")]
    [SerializeField] private bool rightClickCancels = true;

    private DisplayTrain trainDisplayRef;
    private StoreUiInteracts UIRef;
    private ReorderCameraController reorderCameraRef;
    private ReorderHUD hudRef;
    private bool hudSubscribed;

    private enum State { Hovering, Panel, Dragging }
    private State state = State.Hovering;

    private ShopWagonData cacheRef;
    private int hoveredSlot = -1;
    private ReorderHUD.Option selectedOption = ReorderHUD.Option.Move;
    private bool isInReorderMode;

    private const int OptionCount = 3;

    private int outlineLayer;
    private int whiteOutlineLayer;

    private void Awake()
    {
        outlineLayer = LayerMask.NameToLayer("Outline");
        whiteOutlineLayer = LayerMask.NameToLayer("WhiteOutline");
    }

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
        UnsubscribeHud();

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

        if (hudRef != null && !hudSubscribed) SubscribeHud();
    }

    private void SubscribeHud()
    {
        hudRef.OptionHovered += OnHudOptionHovered;
        hudRef.OptionClicked += OnHudOptionClicked;
        hudRef.ArrowClicked += OnHudArrowClicked;
        hudRef.ConfirmClicked += OnHudConfirm;
        hudRef.CancelClicked += OnHudCancel;
        hudRef.OptionUnhovered += OnHudOptionUnhovered;
        hudSubscribed = true;
    }

    private void UnsubscribeHud()
    {
        if (!hudSubscribed || hudRef == null) return;

        hudRef.OptionHovered -= OnHudOptionHovered;
        hudRef.OptionClicked -= OnHudOptionClicked;
        hudRef.ArrowClicked -= OnHudArrowClicked;
        hudRef.ConfirmClicked -= OnHudConfirm;
        hudRef.CancelClicked -= OnHudCancel;
        hudRef.OptionUnhovered -= OnHudOptionUnhovered;
        hudSubscribed = false;
    }

    private void Update()
    {
        if (!isInReorderMode || !rightClickCancels) return;

        var mouse = Mouse.current;
        if (mouse != null && mouse.rightButton.wasPressedThisFrame)
            HandleCancel();
    }

    #endregion

    #region Teclado ? acciones

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        if (!isInReorderMode) return;

        int dir = Mathf.RoundToInt(ctx.ReadValue<Vector2>().x);
        if (dir != 0) HandleDirection(dir);
    }

    private void OnSelectPerformed(InputAction.CallbackContext ctx)
    {
        if (isInReorderMode) HandleSelect();
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (isInReorderMode) HandleCancel();
    }

    #endregion

    #region Mouse (HUD) ? acciones

    private void OnHudOptionHovered(ReorderHUD.Option option)
    {
        if (!isInReorderMode || state == State.Dragging || cacheRef == null) return;

        if (state == State.Panel)
        {
            selectedOption = option;
            RefreshPanel();
        }
        else
        {

            PreviewOption(option);
        }
    }

    private void OnHudOptionUnhovered(ReorderHUD.Option option)
    {

        if (isInReorderMode && state == State.Hovering) ShowHover();
    }

    private void OnHudOptionClicked(ReorderHUD.Option option)
    {
        if (!isInReorderMode || state == State.Dragging || cacheRef == null) return;


        selectedOption = option;
        ExecuteSelectedOption();
    }

    private void OnHudArrowClicked(int dir)
    {
        if (isInReorderMode) HandleDirection(dir);
    }

    private void OnHudConfirm()
    {
        if (isInReorderMode) HandleSelect();
    }

    private void OnHudCancel()
    {
        if (isInReorderMode) HandleCancel();
    }

    #endregion

    #region Acciones compartidas

    private void HandleDirection(int dir)
    {
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

    private void HandleSelect()
    {
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

    private void HandleCancel()
    {
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
        var newRef = trainDisplayRef.InstantiatedWagonReferences[slot];

        if (cacheRef != null && cacheRef != newRef) cacheRef.SetLayer(outlineLayer);

        hoveredSlot = slot;
        cacheRef = newRef;
        cacheRef.SetLayer(whiteOutlineLayer);

        if (moveCamera) reorderCameraRef?.SetTarget(cacheRef.transform);
    }

    private void ShowHover()
    {
        if (hudRef == null || cacheRef == null) return;

        var so = trainDisplayRef.GetWagonSO(hoveredSlot);       
        var baseSo = trainDisplayRef.GetBaseWagonSO(hoveredSlot); 

        string wagonName = so != null ? so.wagonName : cacheRef.IDReference.WagonName;
        string description = baseSo != null ? baseSo.Description : string.Empty;

        GetUpgradeState(out _, out _, out bool canAfford);
        hudRef.ShowHover(cacheRef, wagonName, description, canAfford);
    }

    #endregion

    #region Panel

    private void RefreshPanel()
    {
        if (hudRef == null || cacheRef == null) return;

        string description = BuildOptionDescription(selectedOption, out string wagonName, out bool canAfford);
        hudRef.ShowPanel(cacheRef, wagonName, selectedOption, description, canAfford);
    }

    private void PreviewOption(ReorderHUD.Option option)
    {
        if (hudRef == null || cacheRef == null) return;

        string description = BuildOptionDescription(option, out string wagonName, out bool canAfford);
        hudRef.PreviewOption(wagonName, option, description, canAfford);
    }

    private string BuildOptionDescription(ReorderHUD.Option option, out string wagonName, out bool canAfford)
    {
        var so = trainDisplayRef.GetWagonSO(hoveredSlot);
        wagonName = so != null ? so.wagonName : cacheRef.IDReference.WagonName;

        bool hasUpgrade = GetUpgradeState(out var next, out float cost, out canAfford);

        return option switch
        {
            ReorderHUD.Option.Move => moveDescription,
            ReorderHUD.Option.Upgrade => hasUpgrade ? BuildUpgradeText(next, cost, canAfford) : maxLevelDescription,
            _ => $"Sell for {hudRef.FormatMoney(GetSellValue(cacheRef), true)}"
        };
    }

    private bool GetUpgradeState(out WagonInStockSO next, out float cost, out bool canAfford)
    {
        bool hasUpgrade = trainDisplayRef.TryGetUpgradeInfo(hoveredSlot, out next, out cost);
        canAfford = hasUpgrade && storeManagerRef.GetGold() >= cost;
        return hasUpgrade;
    }

    private string BuildUpgradeText(WagonInStockSO next, float cost, bool canAfford)
    {
        string price = $"Upgrade for {hudRef.FormatMoney(cost, canAfford)}";
        return string.IsNullOrEmpty(next.Description) ? price : $"{next.Description}\n<b>{price}</b>";
    }

    private void ExecuteSelectedOption()
    {
        switch (selectedOption)
        {
            case ReorderHUD.Option.Move:
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
        if (!storeManagerRef.TrySpendGold(cost)) return;

        var upgraded = trainDisplayRef.UpgradeWagon(hoveredSlot);
        if (upgraded == null)
        {
            storeManagerRef.AddGold(cost); 
            return;
        }

        cacheRef = null; 
        SetHovered(hoveredSlot, moveCamera: true);
        state = State.Hovering;
        ShowHover();
    }

    private void SellHovered()
    {
        var sold = trainDisplayRef.SellWagon(hoveredSlot);
        if (sold == null) return;

        storeManagerRef.AddGold(GetSellValue(sold));
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

            int slot = (hoveredSlot < 0 || hoveredSlot >= count) ? 0 : hoveredSlot;
            SetHovered(slot, moveCamera: false);
            reorderCameraRef?.Activate(cacheRef.transform);

            UIRef?.HideUI();

            EventBus.Publish(new OnShowCursorEvent(CursorType.RealAndFrozen));


            isInReorderMode = true;
            ShowHover();
        }
        else
        {
            if (state == State.Dragging) trainDisplayRef.CancelDrag();
            if (cacheRef != null) cacheRef.SetLayer(outlineLayer);

            state = State.Hovering;
            hudRef?.Hide();
            reorderCameraRef?.Deactivate();
            UIRef?.DeactivateUI();

            isInReorderMode = false;
        }

        EventBus.Publish(new OnActivateUiEvent(!toggled));
    }

    #endregion
}