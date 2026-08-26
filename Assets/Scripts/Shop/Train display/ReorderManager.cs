using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReorderManager : MonoBehaviour
{
    [SerializeField] private PlayerInput inputRef;

    private DisplayTrain trainDisplayRef;
    private StoreUiInteracts UIRef;
    private ShopWagonData selected;
    private ShopWagonData objective;
    private int counter;
    private bool isInReorderMode;


    private void Awake()
    {
        ServiceLocator.Register(this);
        ServiceLocator.TryGet<DisplayTrain>(out trainDisplayRef);
        ServiceLocator.TryGet<StoreUiInteracts>(out UIRef);
        counter = 0;
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

        int current = Mathf.RoundToInt(value.ReadValue<Vector2>().x);

        counter += current;
        if (counter > trainDisplayRef.InstantiatedWagonReferences.Count - 1) counter = 0; 
        if (counter < 0) counter = trainDisplayRef.InstantiatedWagonReferences.Count - 1; 


    }

    private void OnJumpPerformed(InputAction.CallbackContext value)
    {
        if (!isInReorderMode) return;
        SelectWagons(counter);
    }

    private void OnPausePerformed(InputAction.CallbackContext value)
    {
        if (!isInReorderMode) return;
        ToggleReorderMode(false);
    }

    public void ToggleReorderMode(bool toggled)
    {
        if (trainDisplayRef == null) ServiceLocator.TryGet<DisplayTrain>(out trainDisplayRef);

        if (UIRef == null) ServiceLocator.TryGet<StoreUiInteracts>(out UIRef);

        if (trainDisplayRef == null) return;

        UIRef.DeactivateUI();

        EventBus.Publish(new OnActivateUiEvent(!toggled));
        isInReorderMode = toggled;
    }

    private void ConfirmSwap()
    {
        if (trainDisplayRef == null) ServiceLocator.TryGet<DisplayTrain>(out trainDisplayRef);

        if (trainDisplayRef == null) return;

        if (selected == null || objective == null) return;

        trainDisplayRef.ReorderWagons(selected, objective);

        selected = null;
        objective = null;
    }

    private void SelectWagons(int selectedID)
    {
        if (selected == null) selected = SelectSingleWagon(selectedID);
        else if (objective == null)
        { 
            objective = SelectSingleWagon(selectedID); 
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
