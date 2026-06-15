using UnityEngine;
using UnityEngine.InputSystem;

public class StatPanelController : MonoBehaviour
{
    [SerializeField] private GameObject statsPanelCanvas;
    [SerializeField] private InputActionAsset inputActions;

    private InputAction toggleAction;

    void Awake()
    {
        toggleAction = inputActions.FindAction("CallUi");
    }

    void OnEnable()
    {
        toggleAction.Enable();
        toggleAction.performed += OnToggleStats;
    }

    void OnDisable()
    {
        toggleAction.performed -= OnToggleStats;
        toggleAction.Disable();
    }

    private void OnToggleStats(InputAction.CallbackContext ctx)
    {
        statsPanelCanvas.SetActive(!statsPanelCanvas.activeSelf);
    }
}
