using UnityEngine;
using UnityEngine.InputSystem;

public class CollectGoldFromWagon : MonoBehaviour
{
    PlayerBrain playerRef;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GoldenWagonBrain goldBrain;
    [SerializeField] private BoxCollider boxCollider;
    InteractInputHandler inputHandler;

    private bool canInteract = false;

    private void Awake()
    {
        EventBus.Subscribe<OnEnemyKilledEvent>(CallCollectGoldEvent);
    }
    void Start()
    {
        boxCollider.enabled = false;

        inputActions.Enable();
        var interactAction = inputActions.FindAction("Player/Interact");
        inputHandler = new InteractInputHandler(interactAction, SetGoldInPlayerInventory);

        if (!GameManager.Instance.IsTutorial) Activate();
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnEnemyKilledEvent>(CallCollectGoldEvent);
        inputHandler.Dispose();
    }

    private void SetGoldInPlayerInventory()
    {
        if (!canInteract) return;

        if (playerRef  != null)
        {
            playerRef.Inventory.GoldAmount += goldBrain.Collector.GiveGold();
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

    private void CallCollectGoldEvent(OnEnemyKilledEvent enemyKillEvent)
    {
        Activate();
    }

    void Activate()
    {
        canInteract = true;
        boxCollider.enabled = true;
    }
}
