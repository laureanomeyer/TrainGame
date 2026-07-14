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
    [SerializeField] private GameObject Interactimage;

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

    private bool IsRepairing = false;
    private bool canAttack = true;

    public PlayerInventory Inventory => inventory;
    public LookObjectToMouse FaceMouse => faceMouse;
    public InteractionUIManager InteractionUIManager => interactionUIManager;
    public PlayerAttackController PlayerAttackController => playerAttackController;
    void Awake() 
    {
        rb = GetComponent<Rigidbody>();

        inventory = new PlayerInventory();
        faceMouse = new LookObjectToMouse(groundMask);
        playerMovementController = new PlayerMovementController(rb, faceMouse, transform, speed);
        playerInteractionsController = new PlayerInteractions(this, playerMovementController, faceMouse, interactionUIManager, repairCapacity);
        playerAttackController = new PlayerAttackController(spawnPoint, weaponItem, GameObject.FindGameObjectWithTag("Factory").GetComponent<BulletPool>(), this, faceMouse);

        attackAction = InputSystem.actions.FindAction("Attack");
        attackAction.performed += ActiveAttack;
        attackAction.canceled += DeactiveAttack;

        EventBus.Subscribe<OnSetAttackEnabledEvent>(CallSetCanAttackEvent);

        EventBus.Subscribe<OnShowInteractEvent>(ShowInteract);
        EventBus.Subscribe<OnHideInteractEvent>(CallHideInteractEvent);
        EventBus.Subscribe<OnActivateUiEvent>(CallSetCanAttackEvent);

        IsRepairing = false;
        HideInteract();
    }
    private void OnDestroy()
    {
        playerInteractionsController.Cleanup();
        attackAction.performed -= ActiveAttack;
        attackAction.canceled -= DeactiveAttack;

        EventBus.Unsubscribe<OnSetAttackEnabledEvent>(CallSetCanAttackEvent);

        EventBus.Unsubscribe<OnShowInteractEvent>(ShowInteract);
        EventBus.Unsubscribe<OnHideInteractEvent>(CallHideInteractEvent);
        EventBus.Unsubscribe<OnActivateUiEvent>(CallSetCanAttackEvent);
    }
    private void Update()
    {
        playerInteractionsController.Update();
        if (!IsRepairing && canAttack) playerAttackController.Update();

        if (Keyboard.current.f8Key.wasPressedThisFrame)
        {
            Inventory.GoldAmount += 100;
        }
    }

    private void FixedUpdate()
    {
        if (!IsRepairing) playerMovementController.FixedUpdate();
    }

    private void OnMove(InputValue value)
    {
        if (playerMovementController != null && value != null)
            playerMovementController.SetMoveInput(value.Get<Vector2>());
    }

    private void OnInteract()
    {
        EventBus.Publish(new OnInteractPressedEvent());

        playerInteractionsController.OnInteract();
    }

    private void OnSkipScene()
    {
        GameManager.Instance.SkipRun();
    }

    private void OnOpenMainMenu()
    {
        playerInteractionsController.OnOpenMainMenu();
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
        playerAttackController.ActiveAttack();
    }
    public void DeactiveAttack(InputAction.CallbackContext context)
    {
        playerAttackController.DeactiveAttack();
    }

    public void CallSetCanAttackEvent(OnActivateUiEvent activateUIEvent)
    {
        SetCanAttack(activateUIEvent.Activated);
    }
    public void CallSetCanAttackEvent(OnSetAttackEnabledEvent AttackEnableEvent)
    {
        SetCanAttack(AttackEnableEvent.Can);
    }

    public void SetCanAttack(bool canAttack)
    {
        this.canAttack = canAttack;
        playerMovementController.SetCanRotate(canAttack);
    }

    public void SetCanMove(bool canMove)
    {
        playerMovementController.SetCanMove(canMove);
    }

    public void SetIsRepairing(bool canAttack)
    {
        this.IsRepairing = canAttack;
    }

    public void ChangeWeapon(GameObject weapon)
    {
        playerAttackController.SetWeapon(weapon);
    }

    private void ShowInteract(OnShowInteractEvent showInteractEvent)
    {
        Interactimage.SetActive(true);
    }

    public void CallHideInteractEvent(OnHideInteractEvent hideInteractEvent)
    {
        HideInteract();
    }

    private void HideInteract()
    {
        Interactimage.SetActive(false);
    }




}
