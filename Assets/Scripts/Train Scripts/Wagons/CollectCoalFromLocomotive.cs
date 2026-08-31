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
        EventBus.Subscribe<OnEnemyKilledEvent>(CallCollectCoalEvent);
    }
    void Start()
    {
        inputActions.Enable();
        var interactAction = inputActions.FindAction("Player/Interact");
        inputHandler = new InteractInputHandler(interactAction, SetCoalInPlayerInventory);

        if (!GameManager.Instance.IsTutorial) Activate();
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnEnemyKilledEvent>(CallCollectCoalEvent);
        inputHandler.Dispose();
    }

    private void SetCoalInPlayerInventory()
    {
        if (!canInteract) return;
        if (!playerRef.Inventory.HasCoal)
        {
            //playerRef.Inventory.CollectCoal();
            coalBrain.CoalCollector.GiveCoal();
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

    private void CallCollectCoalEvent(OnEnemyKilledEvent enemyKillEvent)
    {
        Activate();
    }

    void Activate()
    {
        canInteract = true;
    }
}
