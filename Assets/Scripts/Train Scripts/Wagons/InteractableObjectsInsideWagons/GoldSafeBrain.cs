using UnityEngine;
using UnityEngine.InputSystem;

public class GoldSafeBrain : MonoBehaviour
{
    PlayerBrain playerRef;
    PlayerData playerDataRef;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private BoxCollider boxCollider;
    InteractInputHandler inputHandler;

    [SerializeField] private GameObject objectT;
    [SerializeField] private LayerMask outlineLayer;
    [SerializeField] private LayerMask whiteOutlineLayer;

    [SerializeField] private bool canInteract;
    private bool playerIn = false;

    private float currentGold;
    public float CurrentGold => currentGold;

    private bool firstAdd = false;

    private void Awake()
    {
        currentGold = 0;
        playerDataRef = ServiceLocator.Get<PlayerData>();
        EventBus.Subscribe<OnEnableGoldBoxEvent>(SetCanInteract);
        EventBus.Subscribe<OnSetTutorialFinished>(RestartGoldAmount);


        if (objectT != null)
            objectT.layer = LayerMask.NameToLayer("Outline");
    }

    void Start()
    {
        inputActions.Enable();
        var interactAction = inputActions.FindAction("Player/Interact");
        inputHandler = new InteractInputHandler(interactAction, DepositGoldFromPlayer);

        canInteract = !GameManager.Instance.IsTutorial;
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnEnableGoldBoxEvent>(SetCanInteract);
        EventBus.Unsubscribe<OnSetTutorialFinished>(RestartGoldAmount);

        inputHandler?.Dispose();
    }

    private void DepositGoldFromPlayer()
    {
        if (!canInteract) return;
        if (!playerIn) return;
        if (playerRef == null) return;
        if (!playerRef.CanInteract) return;

        AddGold(playerRef.Inventory.DepositGold());

        EventBus.Publish(new OnDropGoldEvent());
        EventBus.Publish(new OnHideInteractEvent());
        objectT.layer = LayerMask.NameToLayer("Outline");
        EventBus.Publish(new OnSetGoldWaypoint(false));
    }

    public void AddGold(float amount)
    {
        if (amount <= 0) return;

        currentGold += amount;
        EventBus.Publish(new OnGoldBoxChangedEvent(currentGold));
        ChangeGoldInData(amount);

        if (GameManager.Instance.IsTutorial && !firstAdd)
        {
            firstAdd = true;
            EventBus.Publish(new OnAdvanceTutorialStep());
        }
    }

    public void ChangeGoldInData(float amount)
    {
        playerDataRef.AddPlayerGold(amount);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canInteract) return;
        if (!other.gameObject.CompareTag("Player")) return;
        if (!other.TryGetComponent<PlayerBrain>(out playerRef)) return;

        playerIn = true;

        if (playerRef.Inventory.GoldAmount != 0)
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

    void SetCanInteract(OnEnableGoldBoxEvent ev)
    {
        canInteract = ev.Enable;

        if (boxCollider != null)
            boxCollider.enabled = ev.Enable;

        if (!canInteract)
        {
            playerIn = false;
            if (playerRef != null)
            {
                playerRef = null;
                EventBus.Publish(new OnHideInteractEvent());
            }
        }
    }
    /// <summary>
    /// only for tutorial
    /// </summary>
    /// <param name="ev"></param>
    private void RestartGoldAmount(OnSetTutorialFinished ev)
    {
        playerDataRef.ChangePlayerGold(0f);
        EventBus.Publish(new OnGoldBoxChangedEvent(0f));
    }
}