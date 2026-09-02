using UnityEngine;
using UnityEngine.InputSystem;

public class ReorderManager : MonoBehaviour
{
    [SerializeField] private PlayerInput inputRef;

    private DisplayTrain trainDisplayRef;
    private StoreUiInteracts UIRef;
    private ReorderCameraController reorderCameraRef;
    private ShopWagonData selected;
    private ShopWagonData objective;
    private ShopWagonData cacheRef;
    private int selectedWagonKey;
    private int objectiveWagonKey;
    private int currentHoveredWagonKey;
    private bool isInReorderMode;


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

        if (cacheRef != null && cacheRef != selected) SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("Outline"));

        int current = Mathf.RoundToInt(value.ReadValue<Vector2>().x);

        currentHoveredWagonKey -= current;
        if (currentHoveredWagonKey > trainDisplayRef.InstantiatedWagonReferences.Count - 1) currentHoveredWagonKey = 0;
        if (currentHoveredWagonKey < 0) currentHoveredWagonKey = trainDisplayRef.InstantiatedWagonReferences.Count - 1;

        cacheRef = trainDisplayRef.InstantiatedWagonReferences[currentHoveredWagonKey];
        SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("WhiteOutline"));

        reorderCameraRef?.SetTarget(cacheRef.transform);
    }

    private void OnJumpPerformed(InputAction.CallbackContext value)
    {
        if (!isInReorderMode) return;
        SelectWagons(currentHoveredWagonKey);
    }

    private void OnPausePerformed(InputAction.CallbackContext value)
    {
        if (!isInReorderMode) return;
        ToggleReorderMode(false);
    }

    public void ToggleReorderMode(bool toggled)
    {
        if (cacheRef != null && cacheRef != selected) SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("Outline"));

        if (trainDisplayRef == null) ServiceLocator.TryGet<DisplayTrain>(out trainDisplayRef);

        if (UIRef == null) ServiceLocator.TryGet<StoreUiInteracts>(out UIRef);

        if (reorderCameraRef == null) ServiceLocator.TryGet<ReorderCameraController>(out reorderCameraRef);

        if (trainDisplayRef == null) return;

        UIRef.DeactivateUI();

        if (toggled)
        {
            if (currentHoveredWagonKey < 0) currentHoveredWagonKey = 0;
            cacheRef = trainDisplayRef.InstantiatedWagonReferences[currentHoveredWagonKey];
            SetLayerRecursively(cacheRef.gameObject, LayerMask.NameToLayer("WhiteOutline"));
            reorderCameraRef?.Activate(cacheRef.transform);
        }
        else
        {
            reorderCameraRef?.Deactivate();
        }

        EventBus.Publish(new OnActivateUiEvent(!toggled));
        isInReorderMode = toggled;
    }

    private void ConfirmSwap()
    {
        if (trainDisplayRef == null) ServiceLocator.TryGet<DisplayTrain>(out trainDisplayRef);

        if (trainDisplayRef == null) return;

        if (selected == null || objective == null) return;

        trainDisplayRef.ReorderWagons(selected, objective, selectedWagonKey, objectiveWagonKey);

        selected = null;
        objective = null;
    }

    private void SelectWagons(int selectedID)
    {
        if (selected == null)
        {
            selected = SelectSingleWagon(selectedID);
            this.selectedWagonKey = selectedID;
        }
        else if (objective == null)
        {
            objective = SelectSingleWagon(selectedID);
            objectiveWagonKey = selectedID;
            ConfirmSwap();
        }
    }

    private ShopWagonData SelectSingleWagon(int selectedID)
    {
        if (trainDisplayRef == null) ServiceLocator.TryGet<DisplayTrain>(out trainDisplayRef);

        if (trainDisplayRef == null) return null;

        var selected = trainDisplayRef.InstantiatedWagonReferences[selectedID];
        SetLayerRecursively(selected.gameObject, LayerMask.NameToLayer("WhiteOutline"));

        return selected;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        foreach (Transform t in obj.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.layer = layer;
        }
    }
}