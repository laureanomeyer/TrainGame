using UnityEngine;
using UnityEngine.InputSystem;

public class CollectCoalFromLocomotive : MonoBehaviour
{
    PlayerBrain playerRef;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private LocomotiveBrain coalBrain;
    [SerializeField] private BoxCollider boxCollider;
    InteractInputHandler inputHandler;

    private bool canInteract = false;

    private void Awake()
    {
        EventBus.Subscribe<OnEnableCoalBoxEvent>(Activate);
    }
    void Start()
    {
        inputActions.Enable();
        var interactAction = inputActions.FindAction("Player/Interact");
        inputHandler = new InteractInputHandler(interactAction, SetCoalInPlayerInventory);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnEnableCoalBoxEvent>(Activate);
        inputHandler.Dispose();
    }

    private void SetCoalInPlayerInventory()
    {
        if (!canInteract) return;
        if (playerRef == null) return;
        if (!playerRef.Inventory.HasCoal && coalBrain.CoalCollector.HasCoal)
        {
            playerRef.Inventory.CollectCoal();
            EventBus.Publish(new OnTakeCoalEvent());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.TryGetComponent<PlayerBrain>(out playerRef);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerRef = null;
        }
    }
    void Activate(OnEnableCoalBoxEvent ev)
    {
        canInteract = true;
    }
}
