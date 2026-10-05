using UnityEngine;
using UnityEngine.InputSystem;

public class CollectCoalFromLocomotive : MonoBehaviour
{
    PlayerBrain playerRef;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private LocomotiveBrain coalBrain;
    [SerializeField] private BoxCollider boxCollider;
    InteractInputHandler inputHandler;

    [SerializeField] private GameObject objectT;
    [SerializeField] private LayerMask outlineLayer;
    [SerializeField] private LayerMask whiteOutlineLayer;

    [SerializeField] private bool canInteract;
    private bool playerIn = false;

    private void Awake()
    {
        EventBus.Subscribe<OnEnableCoalBoxEvent>(Activate);

        if (objectT != null)
            objectT.layer = LayerMask.NameToLayer("Outline");
    }
    void Start()
    {
        inputActions.Enable();
        var interactAction = inputActions.FindAction("Player/Interact");
        inputHandler = new InteractInputHandler(interactAction, SetCoalInPlayerInventory);

        canInteract = !GameManager.Instance.IsTutorial;
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnEnableCoalBoxEvent>(Activate);
        inputHandler.Dispose();
    }

    private void SetCoalInPlayerInventory()
    {
        if (!canInteract) return;
        if (!playerIn) return;
        if (playerRef == null) return;
        if (!playerRef.CanInteract) return;

        if (!playerRef.Inventory.HasCoal && coalBrain.CoalCollector.HasCoal)
        {
            playerRef.Inventory.CollectCoal();
            EventBus.Publish(new OnTakeCoalEvent());
            EventBus.Publish(new OnHideInteractEvent());
            objectT.layer = LayerMask.NameToLayer("Outline");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canInteract) return;
        if (!other.gameObject.CompareTag("Player")) return;
        if (!other.TryGetComponent<PlayerBrain>(out playerRef)) return;

        playerIn = true;

        if (coalBrain.CoalCollector.HasCoal)
        {
            EventBus.Publish(new OnShowInteractEvent());
            if (objectT != null)
                objectT.layer = LayerMask.NameToLayer("WhiteOutline");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        playerIn = false;
        playerRef = null;

        if (canInteract)
        {
            EventBus.Publish(new OnHideInteractEvent());
            if (objectT != null)
                objectT.layer = LayerMask.NameToLayer("Outline");
        }
    }

    void Activate(OnEnableCoalBoxEvent ev)
    {
        canInteract = true;
    }
}