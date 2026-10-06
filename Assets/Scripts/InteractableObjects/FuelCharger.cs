using UnityEngine;
using UnityEngine.InputSystem;

public class FuelCharger : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private LocomotiveBrain locomotive;
    [SerializeField] private BoxCollider coll;

    [SerializeField] private GameObject objectT;
    [SerializeField] private LayerMask outlineLayer;
    [SerializeField] private LayerMask whiteOutlineLayer;

    InteractInputHandler inputHandler;
    PlayerBrain playerRef;

    private bool canInteract = true;
    private bool playerIn = false;
    private bool firstFill = false;

    private void Awake()
    {
        EventBus.Subscribe<OnEnableCoalBoxEvent>(SetActive);

        if (objectT != null)
            objectT.layer = LayerMask.NameToLayer("Outline");
    }
    void Start()
    {
        inputActions.Enable();
        var interactAction = inputActions.FindAction("Player/Interact");
        inputHandler = new InteractInputHandler(interactAction, OnInteract);
    }
    private void OnDestroy()
    {
        inputHandler.Dispose();
        EventBus.Unsubscribe<OnEnableCoalBoxEvent>(SetActive);
    }

    void OnInteract()
    {
        if (!canInteract) return;
        if (!playerIn) return;
        if (playerRef == null) return;

        AddFuel();
    }

    private void AddFuel()
    {
        if (!canInteract) return;
        if (!playerIn) return;
        if (playerRef == null) return;

        if (playerRef.Inventory.HasCoal)
        {
            locomotive.AddFuel();
            playerRef.Inventory.DepositCoal();
            EventBus.Publish(new OnDropFuelEvent());
            EventBus.Publish(new OnDeactivateFuelChargerWaypoint());
            EventBus.Publish(new OnHideInteractEvent());
            objectT.layer = LayerMask.NameToLayer("Outline");

            if (GameManager.Instance.CurrentState == GameState.Tutorial && firstFill == false)
            {
                EventBus.Publish(new OnAdvanceTutorialStep());
                firstFill = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (!other.TryGetComponent<PlayerBrain>(out playerRef)) return;

        playerIn = true;

        if (canInteract && playerRef.Inventory.HasCoal)
        {
            EventBus.Publish(new OnShowInteractEvent());
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
            objectT.layer = LayerMask.NameToLayer("Outline");
        }
    }

    void SetActive(OnEnableCoalBoxEvent enableCoalBoxEvent)
    {
        canInteract = enableCoalBoxEvent.Enable;
        coll.enabled = enableCoalBoxEvent.Enable;

        if (!canInteract)
        {
            playerIn = false;
            playerRef = null;
        }
    }
}