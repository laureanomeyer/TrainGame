using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class InteractInputHandler
{
    private InputAction interactAction;
    private Action<InputAction.CallbackContext> callback;

    public InteractInputHandler(InputAction interactAction, Action onInteract)
    {
        this.interactAction = interactAction;
        callback = _ => onInteract?.Invoke();             
        this.interactAction.performed += callback;
        this.interactAction.Enable();
    }
    public void Dispose()
    {
        interactAction.performed -= callback;       
        interactAction.Disable();
    }
}

