using UnityEngine;
using UnityEngine.InputSystem;

public class CollectGoldFromWagon : MonoBehaviour
{
    PlayerBrain playerRef;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GoldenWagonBrain goldBrain;
    [SerializeField] private BoxCollider boxCollider;
    InteractInputHandler inputHandler;

    [SerializeField] private GameObject objectT;
    [SerializeField] private LayerMask outlineLayer;
    [SerializeField] private LayerMask whiteOutlineLayer;

    private bool canInteract = false;
    private bool playerIn = false;

    private void Awake()
    {
        EventBus.Subscribe<OnEnemyKilledEvent>(CallCollectGoldEvent);
        EventBus.Subscribe<OnEnableGoldBoxEvent>(Activate);

        if (objectT != null)
            objectT.layer = LayerMask.NameToLayer("Outline");
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
        EventBus.Unsubscribe<OnEnableGoldBoxEvent>(Activate);
        inputHandler.Dispose();
    }

    private void SetGoldInPlayerInventory()
    {
        if (!canInteract) return;
        if (!playerIn) return;
        if (playerRef == null) return;
        if (!playerRef.CanInteract) return;

        playerRef.Inventory.GoldAmount += goldBrain.Collector.GiveGold();
        EventBus.Publish(new OnHideInteractEvent());
        objectT.layer = LayerMask.NameToLayer("Outline");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canInteract) return;
        if (!other.gameObject.CompareTag("Player")) return;
        if (!other.TryGetComponent<PlayerBrain>(out playerRef)) return;

        playerIn = true;

        if (goldBrain.Collector.Gold != 0)
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

    private void CallCollectGoldEvent(OnEnemyKilledEvent enemyKillEvent)
    {
        Activate();
    }

    void Activate()
    {
        canInteract = true;
        boxCollider.enabled = true;
    }
    void Activate(OnEnableGoldBoxEvent ev)
    {
        canInteract = true;
        boxCollider.enabled = true;
    }
}