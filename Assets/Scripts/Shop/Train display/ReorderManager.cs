using UnityEngine;
using UnityEngine.InputSystem;

public class ReorderManager : MonoBehaviour
{
    [SerializeField] private PlayerInput inputRef;

    private DisplayTrain trainDisplayRef;
    private StoreUiInteracts UIRef;
    private ReorderCameraController reorderCameraRef;

    private ShopWagonData cacheRef;
    private int currentHoveredWagonKey;
    private bool isInReorderMode;
    private bool isDragging;

    private void Awake()
    {
        ServiceLocator.Register(this);
        ServiceLocator.TryGet<DisplayTrain>(out trainDisplayRef);
        ServiceLocator.TryGet<StoreUiInteracts>(out UIRef);
        ServiceLocator.TryGet<ReorderCameraController>(out reorderCameraRef);
        currentHoveredWagonKey = -1;
    }

    private void OnEnable()
    {
        inputRef.actions["Move"].performed += OnMovePerformed;
        inputRef.actions["Jump"].performed += OnJumpPerformed;
        inputRef.actions["Pause"].performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        if (inputRef == null) return;
        inputRef.actions["Move"].performed -= OnMovePerformed;
        inputRef.actions["Jump"].performed -= OnJumpPerformed;
        inputRef.actions["Pause"].performed -= OnPausePerformed;
    }

    private void OnMovePerformed(InputAction.CallbackContext value)
    {
        if (!isInReorderMode) return;

        int direction = Mathf.RoundToInt(value.ReadValue<Vector2>().x);
        if (direction == 0) return;

        if (isDragging)
        {
            if (trainDisplayRef.StepDrag(direction))
            {
                currentHoveredWagonKey = trainDisplayRef.DraggedSlot;
                reorderCameraRef?.SetTarget(trainDisplayRef.DraggedWagon.transform);
            }
            return;
        }

        // Navegaci?n de hover (sin agarrar wagon todav?a)
        if (cacheRef != null) SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("Outline"));

        currentHoveredWagonKey -= direction;
        if (currentHoveredWagonKey > trainDisplayRef.InstantiatedWagonReferences.Count - 1) currentHoveredWagonKey = 0;
        if (currentHoveredWagonKey < 0) currentHoveredWagonKey = trainDisplayRef.InstantiatedWagonReferences.Count - 1;

        cacheRef = trainDisplayRef.InstantiatedWagonReferences[currentHoveredWagonKey];
        SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("WhiteOutline"));

        reorderCameraRef?.SetTarget(cacheRef.transform);
    }

    private void OnJumpPerformed(InputAction.CallbackContext value)
    {
        if (!isInReorderMode) return;

        if (!isDragging)
        {
            trainDisplayRef.CacheSlotLayout();
            trainDisplayRef.BeginDrag(currentHoveredWagonKey);
            isDragging = true;
        }
        else
        {
            trainDisplayRef.EndDrag();
            isDragging = false;
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext value)
    {
        if (!isInReorderMode) return;

        // Si sueltan pausa en medio de un drag, lo confirmamos en su lugar actual
        if (isDragging)
        {
            trainDisplayRef.EndDrag();
            isDragging = false;
        }

        ToggleReorderMode(false);
    }

    public void ToggleReorderMode(bool toggled)
    {
        if (trainDisplayRef.InstantiatedWagonReferences.Count <= 0) return;

        if (cacheRef != null) SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("Outline"));

        if (trainDisplayRef == null) ServiceLocator.TryGet<DisplayTrain>(out trainDisplayRef);
        if (UIRef == null) ServiceLocator.TryGet<StoreUiInteracts>(out UIRef);
        if (reorderCameraRef == null) ServiceLocator.TryGet<ReorderCameraController>(out reorderCameraRef);

        if (trainDisplayRef == null) return;

        if (toggled)
        {
            if (currentHoveredWagonKey < 0) currentHoveredWagonKey = 0;

            trainDisplayRef.CacheSlotLayout();

            cacheRef = trainDisplayRef.InstantiatedWagonReferences[currentHoveredWagonKey];
            SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("WhiteOutline"));
            reorderCameraRef?.Activate(cacheRef.transform);
            UIRef.HideUI();
        }
        else
        {
            reorderCameraRef?.Deactivate();
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