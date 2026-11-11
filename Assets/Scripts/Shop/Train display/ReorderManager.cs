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
    [Tooltip("Texto de la mejora del vagón de oro. {0} = bonus de oro del próximo nivel.")]
    [SerializeField] private string goldUpgradeFormat = "+{0} gold multiplier";

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
        if (!IsOptionAvailable(option)) return;

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
        if (!IsOptionAvailable(option)) return;

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
                selectedOption = NextAvailableOption(dir);
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
                selectedOption = IsGoldHovered ? ReorderHUD.Option.Upgrade : ReorderHUD.Option.Move;
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
        int count = trainDisplayRef.SelectableCount;
        if (count == 0) return;

        int slot = hoveredSlot - dir;
        if (slot > count - 1) slot = 0;
        if (slot < 0) slot = count - 1;

        SetHovered(slot, moveCamera: true);
        ShowHover();
    }

    private void SetHovered(int slot, bool moveCamera)
    {
        var newRef = trainDisplayRef.GetSelectable(slot);
        if (newRef == null) return;

        if (cacheRef != null && cacheRef != newRef) cacheRef.SetLayer(outlineLayer);

        hoveredSlot = slot;
        cacheRef = newRef;
        cacheRef.SetLayer(whiteOutlineLayer);

        if (moveCamera) reorderCameraRef?.SetTarget(cacheRef.transform);
    }

    private void ShowHover()
    {
        if (hudRef == null || cacheRef == null) return;

        string wagonName = GetHoveredName();
        string description;

        if (IsGoldHovered)
        {
            description = trainDisplayRef.GoldWagonDescription;
        }
        else
        {
            var baseSo = trainDisplayRef.GetBaseWagonSO(hoveredSlot);
            description = baseSo != null ? baseSo.Description : string.Empty;
        }

        GetUpgradeState(out _, out _, out bool canAfford);
        hudRef.ShowHover(cacheRef, wagonName, description, canAfford, !IsGoldHovered);
    }

    private string GetHoveredName()
    {
        if (IsGoldHovered)
        {
            // 0 mejoras = "Gold Wagon", 1 mejora = "Gold Wagon II", 2 = "III"...
            int displayLevel = trainDisplayRef.GetGoldWagonLevel() + 1;
            string baseName = trainDisplayRef.GoldWagonName;
            return displayLevel <= 1 ? baseName : $"{baseName} {ToRoman(displayLevel)}";
        }

        var so = trainDisplayRef.GetWagonSO(hoveredSlot);
        return so != null ? so.wagonName : cacheRef.IDReference.WagonName;
    }

    #endregion

    #region Panel

    private void RefreshPanel()
    {
        if (hudRef == null || cacheRef == null) return;

        string description = BuildOptionDescription(selectedOption, out string wagonName, out bool canAfford);
        hudRef.ShowPanel(cacheRef, wagonName, selectedOption, description, canAfford, !IsGoldHovered);
    }

    private void PreviewOption(ReorderHUD.Option option)
    {
        if (hudRef == null || cacheRef == null) return;

        string description = BuildOptionDescription(option, out string wagonName, out bool canAfford);
        hudRef.PreviewOption(wagonName, option, description, canAfford, !IsGoldHovered);
    }

    private string BuildOptionDescription(ReorderHUD.Option option, out string wagonName, out bool canAfford)
    {
        wagonName = GetHoveredName();

        bool hasUpgrade = GetUpgradeState(out var next, out float cost, out canAfford);

        if (!IsOptionAvailable(option)) return string.Empty;

        return option switch
        {
            ReorderHUD.Option.Move => moveDescription,
            ReorderHUD.Option.Upgrade => !hasUpgrade
                ? maxLevelDescription
                : IsGoldHovered ? BuildGoldUpgradeText(cost, canAfford) : BuildUpgradeText(next, cost, canAfford),
            _ => $"Sell for {hudRef.FormatMoney(GetSellValue(cacheRef), true)}"
        };
    }

    private bool GetUpgradeState(out WagonInStockSO next, out float cost, out bool canAfford)
    {
        next = null;
        bool hasUpgrade = IsGoldHovered
            ? trainDisplayRef.TryGetGoldUpgradeInfo(out cost, out _)
            : trainDisplayRef.TryGetUpgradeInfo(hoveredSlot, out next, out cost);

        canAfford = hasUpgrade && storeManagerRef.GetGold() >= cost;
        return hasUpgrade;
    }

    private string BuildGoldUpgradeText(float cost, bool canAfford)
    {
        trainDisplayRef.TryGetGoldUpgradeInfo(out _, out float bonus);
        string price = $"Upgrade for {hudRef.FormatMoney(cost, canAfford)}";
        return $"{string.Format(goldUpgradeFormat, bonus)}\n\n<b>{price}</b>";
    }

    private string BuildUpgradeText(WagonInStockSO next, float cost, bool canAfford)
    {
        string price = $"Upgrade for {hudRef.FormatMoney(cost, canAfford)}";
        return string.IsNullOrEmpty(next.Description) ? price : $"{next.Description}\n\n<b>{price}</b>";
    }

    private void ExecuteSelectedOption()
    {
        if (!IsOptionAvailable(selectedOption)) return;

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
        if (IsGoldHovered)
        {
            TryUpgradeGold();
            return;
        }

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

    private void TryUpgradeGold()
    {
        if (!trainDisplayRef.TryGetGoldUpgradeInfo(out float cost, out _)) return;
        if (!storeManagerRef.TrySpendGold(cost)) return;

        if (!trainDisplayRef.UpgradeGoldWagon())
        {
            storeManagerRef.AddGold(cost);
            return;
        }

        state = State.Hovering;
        ShowHover();
    }

    private void SellHovered()
    {
        var sold = trainDisplayRef.SellWagon(hoveredSlot);
        if (sold == null) return;

        storeManagerRef.AddGold(GetSellValue(sold));
        cacheRef = null;

        int count = trainDisplayRef.SelectableCount;
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

    #region Gold wagon helpers

    private bool IsGoldHovered => trainDisplayRef != null && trainDisplayRef.IsGoldSlot(hoveredSlot);

    // El vagón de oro solo se puede mejorar: no se mueve ni se vende.
    private bool IsOptionAvailable(ReorderHUD.Option option) =>
        !IsGoldHovered || option == ReorderHUD.Option.Upgrade;

    private static readonly int[] RomanValues = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
    private static readonly string[] RomanSymbols = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

    private static string ToRoman(int number)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < RomanValues.Length && number > 0; i++)
        {
            while (number >= RomanValues[i])
            {
                sb.Append(RomanSymbols[i]);
                number -= RomanValues[i];
            }
        }
        return sb.ToString();
    }

    private ReorderHUD.Option NextAvailableOption(int dir)
    {
        int index = (int)selectedOption;

        for (int i = 0; i < OptionCount; i++)
        {
            index = (index + dir + OptionCount) % OptionCount;
            var option = (ReorderHUD.Option)index;
            if (IsOptionAvailable(option)) return option;
        }

        return selectedOption;
    }

    #endregion

    #region Mode toggle

    public void ToggleReorderMode(bool toggled)
    {
        ResolveRefs();
        if (trainDisplayRef == null) return;
        if (toggled == isInReorderMode) return;

        if (toggled)
        {
            AnalyticsManager.Instance.RegisterWagonManagerInteract();

            int count = trainDisplayRef.SelectableCount;
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