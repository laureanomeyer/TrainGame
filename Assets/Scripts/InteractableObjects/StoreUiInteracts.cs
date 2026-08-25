using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StoreUiInteracts : MonoBehaviour
{
    [SerializeField] private GameObject uiToShow;

    [SerializeField] private GameObject[] interactObjects;

    [SerializeField] private LayerMask noInteractLayer;
    [SerializeField] private LayerMask interactLayer;

    [SerializeField] private Button closeButton;

    private bool playerInZone;
    private bool uiOpen = false;    

    private void OnEnable()
    {
        EventBus.Subscribe<OnInteractPressedEvent>(OnPlayerInteractEvent);
        closeButton.onClick.AddListener(DeactivateUI);
        ServiceLocator.Register(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnInteractPressedEvent>(OnPlayerInteractEvent);
        closeButton.onClick.RemoveListener(DeactivateUI);
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

        foreach (GameObject obj in interactObjects)
        {
            obj.layer = LayerMaskToLayer(interactLayer);
        }

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
        uiToShow.SetActive(false);

        EventBus.Publish(new OnHideInteractEvent());
        EventBus.Publish(new OnShowCursorEvent(CursorType.Gameplay));
    }

    public void DeactivateUI()
    {
        uiToShow.SetActive(false);

        GameManager.Instance.ChangeGameState(GameState.Gameplay);

        EventBus.Publish(new OnShowInteractEvent());
        EventBus.Publish(new OnShowCursorEvent(CursorType.Gameplay));
        EventBus.Publish(new OnActivateUiEvent(true));
    }

    public void OnPlayerInteractEvent(OnInteractPressedEvent interactPressedEvent)
    {
        OnPlayerInteract();
    }

    private void OnPlayerInteract()
    {
        if (!playerInZone) return;

        if (!uiOpen)
        {
            uiOpen = true;

            uiToShow.SetActive(true);

            EventBus.Publish(new OnShowCursorEvent(CursorType.Real));
            EventBus.Publish(new OnActivateUiEvent(false));

            GameManager.Instance.ChangeGameState(GameState.UI);
        }
        else
        {
            uiOpen = false;

            DeactivateUI();
        }
    }
}
