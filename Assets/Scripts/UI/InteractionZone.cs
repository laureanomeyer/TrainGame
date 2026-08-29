using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Timeline.DirectorControlPlayable;

public class InteractionZone : MonoBehaviour
{
    public enum ZoneType { Text, Buttons }

    [SerializeField] private ZoneType zoneType;
    [SerializeField] public string message;
    [SerializeField] private GameObject optionalPanel = null;

    [SerializeField] private GameObject[] interactObjects;

    [SerializeField] private LayerMask noInteractLayer;
    [SerializeField] private LayerMask interactLayer;

    private bool playerInZone;
    private InteractionUIManager ui;
    private PlayerBrain playerBrain;
    private InputAction pauseAction;

    private bool isOpen = false;

    private void OnEnable()
    {
        EventBus.Subscribe<OnInteractPressedEvent>(CallOnPlayerInteractEvent);
        pauseAction = InputSystem.actions.FindAction("Pause");

        if (pauseAction != null)
        {
            pauseAction.performed += OnPausePressed;
        }
    }
    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePressed;
        }
    }
    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnInteractPressedEvent>(CallOnPlayerInteractEvent);
    }

    private int LayerMaskToLayer(LayerMask mask)
    {
        int layerNumber = 0;
        int layer = mask.value;
        while (layer > 1)
        {
            layer >>= 1;
            layerNumber++;
        }
        return layerNumber;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!other.TryGetComponent(out playerBrain)) return;

        foreach (GameObject obj in interactObjects)
        {
            obj.layer = LayerMaskToLayer(interactLayer);
        }

        ui = playerBrain.InteractionUIManager;
        playerInZone = true;
        EventBus.Publish(new OnShowInteractEvent());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (GameObject obj in interactObjects)
        {
            obj.layer = LayerMaskToLayer(noInteractLayer);
        }

        playerInZone = false;
        isOpen = false; 
        ui?.HideAll();
        if (optionalPanel != null)
            optionalPanel.SetActive(false);

        //playerBrain.SetCanMove(true);

        playerBrain = null;
        ui = null;

        EventBus.Publish(new OnActivateUiEvent(true));
        EventBus.Publish(new OnActivateNonPausableUI(true));
        EventBus.Publish(new OnHideInteractEvent());
        EventBus.Publish(new OnShowCursorEvent(CursorType.Gameplay));
    }

    public void DeactivateUI()
    {
        isOpen = false;
        ui?.HideAll();
        if (optionalPanel != null)
            optionalPanel.SetActive(false);

        // playerBrain.SetCanMove(true);

        GameManager.Instance.ChangeGameState(GameState.Gameplay);

        EventBus.Publish(new OnActivateUiEvent(true));
        EventBus.Publish(new OnActivateNonPausableUI(true));
        EventBus.Publish(new OnShowInteractEvent());
        EventBus.Publish(new OnShowCursorEvent(CursorType.Gameplay));
    }

    private void CallOnPlayerInteractEvent(OnInteractPressedEvent interactPressedEvent)
    {
        OnPlayerInteract();
    }

    private void OnPlayerInteract()
    {
        if (!playerInZone || ui == null) return;

        if (!isOpen)
        {
            isOpen = true;

            string textToShow = null;

            if (TryGetComponent<WagonShopButton>(out WagonShopButton shopButton))
            {
                shopButton.UpdateUI();
                textToShow = shopButton.DescriptionText;
            }

            if (zoneType == ZoneType.Text)
                ui.ShowText(textToShow);
            else if (zoneType == ZoneType.Buttons)
                ui.ShowButtons();

            if (optionalPanel != null)
                optionalPanel.SetActive(true);

            //playerBrain.SetCanMove(false);

            EventBus.Publish(new OnActivateUiEvent(false));
            EventBus.Publish(new OnActivateNonPausableUI(false));
            EventBus.Publish(new OnHideInteractEvent());
            EventBus.Publish(new OnShowCursorEvent(CursorType.Real));

            GameManager.Instance.ChangeGameState(GameState.UI);
        }
    }

    private void OnPausePressed(InputAction.CallbackContext context)
    {
        if (isOpen)
        {
            DeactivateUI();
        }
    }
}