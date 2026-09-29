using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBrain : MonoBehaviour
{
    [Header("Mouse Controller")]
    [SerializeField] private LayerMask groundMask;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    private Rigidbody rb;

    [Header("Interactions")]
    [SerializeField] private float repairCapacity;
    [SerializeField] private InteractionUIManager interactionUIManager;
    [SerializeField] private GameObject interactImage;

    [Header("Bullets")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject weaponItem;
    public GameObject WeaponItem => weaponItem;

    private PlayerInventory inventory;
    private LookObjectToMouse faceMouse;
    private PlayerMovementController playerMovementController;
    private PlayerInteractions playerInteractionsController;
    private PlayerAttackController playerAttackController;
    private InputAction attackAction;

    private PlayerData playerDataRef;

    private bool isRepairing = false;
    private bool canAttack = true;
    private bool canInteract = true;
    private bool canRepair = true;
    private bool higherLevelFrozen = false;

    public PlayerInventory Inventory => inventory;
    public LookObjectToMouse FaceMouse => faceMouse;
    public InteractionUIManager InteractionUIManager => interactionUIManager;
    public PlayerAttackController PlayerAttackController => playerAttackController;
    public bool IsRepairing => isRepairing;
    public bool CanRepair => canRepair;
    public bool CanInteract => canInteract;
    void Awake() 
    {
        rb = GetComponent<Rigidbody>();

        inventory = new PlayerInventory();
        faceMouse = new LookObjectToMouse(groundMask);
        playerMovementController = new PlayerMovementController(rb, faceMouse, transform, speed);
        playerInteractionsController = new PlayerInteractions(this, playerMovementController, faceMouse, interactionUIManager, repairCapacity);
        playerAttackController = new PlayerAttackController(spawnPoint, weaponItem, GameObject.FindGameObjectWithTag("Factory").GetComponent<BulletPool>(), this, faceMouse);

        playerDataRef = ServiceLocator.Get<PlayerData>();

        attackAction = InputSystem.actions.FindAction("Attack");
        attackAction.performed += ActiveAttack;
        attackAction.canceled += DeactiveAttack;

        EventBus.Subscribe<OnSetAttackEnabledEvent>(CallSetCanAttackEvent);
        EventBus.Subscribe<OnShowInteractEvent>(ShowInteract);
        EventBus.Subscribe<OnHideInteractEvent>(CallHideInteractEvent);
        EventBus.Subscribe<OnActivateUiEvent>(HandleAttackAndMovementInUi);
        EventBus.Subscribe<OnFreezePlayerEvent>(HandleAttackAndMovementInTutorial);

        isRepairing = false;
        HideInteract();

    }
    private void OnDestroy()
    {
        playerAttackController.DestroyWeapon();

        playerInteractionsController.Cleanup();
        attackAction.performed -= ActiveAttack;
        attackAction.canceled -= DeactiveAttack;

        EventBus.Unsubscribe<OnSetAttackEnabledEvent>(CallSetCanAttackEvent);

        EventBus.Unsubscribe<OnShowInteractEvent>(ShowInteract);
        EventBus.Unsubscribe<OnHideInteractEvent>(CallHideInteractEvent);
        EventBus.Unsubscribe<OnActivateUiEvent>(HandleAttackAndMovementInUi);
    }
    private void Update()
    {
        playerInteractionsController.Update();
        if (!isRepairing && canAttack) playerAttackController.Update();

        if (Keyboard.current.f8Key.wasPressedThisFrame)
        {
            playerDataRef.AddPlayerGold(100);
        }
    }

    private void FixedUpdate()
    {
        if (!isRepairing) playerMovementController.FixedUpdate();
    }

    private void OnMove(InputValue value)
    {
        if (playerMovementController != null && value != null)
            playerMovementController.SetMoveInput(value.Get<Vector2>());
    }

    private void OnInteract()
    {
        if (!canInteract) return;
        EventBus.Publish(new OnInteractPressedEvent());

        playerInteractionsController.OnInteract();
    }

    private void OnAdvanceStep()
    {

        EventBus.Publish(new OnAdvanceTutorialStepByClick());
    }

    private void OnSkipTutorial(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("Holdeado!");
            //EventBus.Publish(new OnForceTutorialStepEvent(17));
        }
    }

    private void OnSkipScene()
    {
        GameManager.Instance.SkipRun();
    }

    private void OnOpenMainMenu()
    {
        playerInteractionsController.OnOpenMainMenu();
    }

    public GameObject SpawnWeapon(GameObject weapon)
    {
        return Instantiate(weapon, spawnPoint);
    }

    public void DestroyWeapon(GameObject weapon)
    {
         Destroy(weapon);
    }

    private void OnTriggerEnter(Collider other)
    {
        playerInteractionsController.OnTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        playerInteractionsController.OnTriggerExit(other);
    }

    public void ActiveAttack(InputAction.CallbackContext context)
    {
        if (!canAttack) return;
        playerAttackController.ActiveAttack();
    }
    public void DeactiveAttack(InputAction.CallbackContext context)
    {
        playerAttackController.DeactiveAttack();
    }
    /// <summary>
    /// Congela al player cuando se abre una UI
    /// </summary>
    /// <param name="activateUIEvent"></param>
    public void HandleAttackAndMovementInUi(OnActivateUiEvent activateUIEvent)
    {
        SetCanAttack(activateUIEvent.Activated);
        SetCanMove(activateUIEvent.Activated);
    }

    /// <summary>
    /// Congela al player cuando el tutorial lo pide
    /// </summary>
    /// <param name="ev"></param>
    public void HandleAttackAndMovementInTutorial(OnFreezePlayerEvent ev)
    {
        SetCanAttack(ev.Activated);
        SetCanMove(ev.Activated);
        canRepair = ev.Activated;
        canInteract = ev.Activated;

        higherLevelFrozen = !ev.Activated;
    }

    /// <summary>
    /// Cancela el ataque del player
    /// </summary>
    /// <param name="AttackEnableEvent"></param>
    public void CallSetCanAttackEvent(OnSetAttackEnabledEvent AttackEnableEvent)
    {
        SetCanAttack(AttackEnableEvent.Can);
    }

    public void TryRegainMovementAfterRepairing()
    {
        SetIsRepairing(false);

        if(higherLevelFrozen) return;

        playerMovementController.SetCanMove(true);
        playerMovementController.SetCanRotate(true);
    }
    public void SetCanAttack(bool canAttack)
    {
        this.canAttack = canAttack;
        playerMovementController.SetCanRotate(canAttack);
    }

    private void SetCanMove(bool canMove)
    {
        playerMovementController.SetCanMove(canMove);
    }

    public void SetIsRepairing(bool isRepairing)
    {
        this.isRepairing = isRepairing;
    }

    public void ChangeWeapon(GameObject weapon)
    {
        playerAttackController.SetWeapon(weapon);
    }

    private void ShowInteract(OnShowInteractEvent showInteractEvent)
    {
        if (!canInteract) return;

        interactImage.SetActive(true);
    }

    public void CallHideInteractEvent(OnHideInteractEvent hideInteractEvent)
    {
        HideInteract();
    }

    private void HideInteract()
    {
        if (!canInteract) return;

        interactImage.SetActive(false);
    }
}
