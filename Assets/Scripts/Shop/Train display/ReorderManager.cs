using UnityEngine;
using UnityEngine.InputSystem;

public class ReorderManager : MonoBehaviour
{
    [SerializeField] private PlayerInput inputRef;
    [SerializeField] private float sellRefundFraction = 0.5f;

    private DisplayTrain trainDisplayRef;
    private StoreUiInteracts UIRef;
    private ReorderCameraController reorderCameraRef;
    private WagonManagementPanel panelRef;

    private enum WagonManagementState { Hovering, Panel, Dragging }
    private WagonManagementState managementState = WagonManagementState.Hovering;

    private ShopWagonData cacheRef;
    private int currentHoveredWagonKey;
    private bool isInReorderMode;

    private void Awake()
    {
        ServiceLocator.Register(this);
        ServiceLocator.TryGet(out trainDisplayRef);
        ServiceLocator.TryGet(out UIRef);
        ServiceLocator.TryGet(out reorderCameraRef);
        ServiceLocator.TryGet(out panelRef);
        currentHoveredWagonKey = -1;
    }

    private void OnEnable()
    {
        inputRef.actions["Move"].performed += OnMovePerformed;
        inputRef.actions["Jump"].performed += OnSelectPerformed;
        inputRef.actions["Pause"].performed += OnPausePerformed;
        inputRef.actions["Repair"].performed += OnSellHotkeyPerformed;
    }

    private void OnDisable()
    {
        if (inputRef == null) return;
        inputRef.actions["Move"].performed -= OnMovePerformed;
        inputRef.actions["Jump"].performed -= OnSelectPerformed;
        inputRef.actions["Pause"].performed -= OnPausePerformed;
        inputRef.actions["Repair"].performed -= OnSellHotkeyPerformed;
    }

    private void OnMovePerformed(InputAction.CallbackContext value)
    {
        if (!isInReorderMode) return;
        if (managementState == WagonManagementState.Panel) return;

        int direction = Mathf.RoundToInt(value.ReadValue<Vector2>().x);
        if (direction == 0) return;

        if (managementState == WagonManagementState.Dragging)
        {
            if (trainDisplayRef.StepDrag(direction))
            {
                currentHoveredWagonKey = trainDisplayRef.DraggedSlot;
                reorderCameraRef?.SetTarget(trainDisplayRef.DraggedWagon.transform);
            }
            return;
        }

        // Hovering: navegación entre wagones
        if (cacheRef != null) SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("Outline"));

        currentHoveredWagonKey -= direction;
        int count = trainDisplayRef.InstantiatedWagonReferences.Count;
        if (currentHoveredWagonKey > count - 1) currentHoveredWagonKey = 0;
        if (currentHoveredWagonKey < 0) currentHoveredWagonKey = count - 1;

        cacheRef = trainDisplayRef.InstantiatedWagonReferences[currentHoveredWagonKey];
        SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("WhiteOutline"));

        reorderCameraRef?.SetTarget(cacheRef.transform);
    }

    private void OnSelectPerformed(InputAction.CallbackContext value)
    {
        if (!isInReorderMode) return;

        switch (managementState)
        {
            case WagonManagementState.Hovering:
                if (cacheRef == null) return;
                managementState = WagonManagementState.Panel;
                panelRef?.Show();
                break;

            case WagonManagementState.Dragging:
                trainDisplayRef.EndDrag();
                managementState = WagonManagementState.Hovering;
                break;
        }
    }

    private void OnSellHotkeyPerformed(InputAction.CallbackContext value)
    {
        if (!isInReorderMode) return;
        if (managementState != WagonManagementState.Hovering) return;

        SellHoveredWagon();
    }

    private void OnPausePerformed(InputAction.CallbackContext value)
    {
        if (!isInReorderMode) return;

        if (managementState == WagonManagementState.Dragging)
        {
            trainDisplayRef.EndDrag();
        }
        else if (managementState == WagonManagementState.Panel)
        {
            panelRef?.Hide();
        }

        managementState = WagonManagementState.Hovering;
        ToggleReorderMode(false);
    }

    // Llamado desde WagonManagementPanel (botón Move)
    public void ConfirmMoveSelected()
    {
        panelRef?.Hide();

        trainDisplayRef.CacheSlotLayout();
        trainDisplayRef.BeginDrag(currentHoveredWagonKey);
        managementState = WagonManagementState.Dragging;
    }

    // Llamado desde WagonManagementPanel (botón Sell)
    public void ConfirmSellSelected()
    {
        panelRef?.Hide();
        SellHoveredWagon();
    }

    private void SellHoveredWagon()
    {
        if (currentHoveredWagonKey < 0) return;

        ShopWagonData sold = trainDisplayRef.SellWagon(currentHoveredWagonKey);
        managementState = WagonManagementState.Hovering;

        if (sold == null) return;

        StoreManager.Instance.AddGold(sold.IDReference.Price * sellRefundFraction);

        int count = trainDisplayRef.InstantiatedWagonReferences.Count;
        if (count == 0)
        {
            cacheRef = null;
            currentHoveredWagonKey = -1;
            ToggleReorderMode(false);
            return;
        }

        if (currentHoveredWagonKey >= count) currentHoveredWagonKey = count - 1;

        cacheRef = trainDisplayRef.InstantiatedWagonReferences[currentHoveredWagonKey];
        SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("WhiteOutline"));
        reorderCameraRef?.SetTarget(cacheRef.transform);
    }

    public void ToggleReorderMode(bool toggled)
    {
        if (toggled && trainDisplayRef.InstantiatedWagonReferences.Count <= 0) return;

        if (cacheRef != null) SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("Outline"));

        if (trainDisplayRef == null) ServiceLocator.TryGet(out trainDisplayRef);
        if (UIRef == null) ServiceLocator.TryGet(out UIRef);
        if (reorderCameraRef == null) ServiceLocator.TryGet(out reorderCameraRef);
        if (panelRef == null) ServiceLocator.TryGet(out panelRef);

        if (trainDisplayRef == null) return;

        if (toggled)
        {
            managementState = WagonManagementState.Hovering;

            if (currentHoveredWagonKey < 0 || currentHoveredWagonKey >= trainDisplayRef.InstantiatedWagonReferences.Count)
                currentHoveredWagonKey = 0;

            trainDisplayRef.CacheSlotLayout();

            cacheRef = trainDisplayRef.InstantiatedWagonReferences[currentHoveredWagonKey];
            SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("WhiteOutline"));
            reorderCameraRef?.Activate(cacheRef.transform);
            UIRef.HideUI();
        }
        else
        {
            panelRef?.Hide();
            reorderCameraRef?.Deactivate();
            UIRef?.DeactivateUI(); 
        }

        EventBus.Publish(new OnActivateUiEvent(!toggled));
        isInReorderMode = toggled;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        foreach (Transform t in obj.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.layer = layer;
        }
    }
}