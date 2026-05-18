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

    [Header("Bullets")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject weaponItem;

    private PlayerInventory inventory;
    private LookObjectToMouse faceMouse;
    private PlayerMovementController playerMovementController;
    private PlayerInteractions playerInteractionsController;
    private PlayerAttackController playerAttackController;
    private InputAction repairAction;

    private bool canAttack = true;

    public bool playerCanMove => playerMovementController.CanMove;
    public PlayerInventory Inventory => inventory;
    public LookObjectToMouse FaceMouse => faceMouse;
    public InteractionUIManager InteractionUIManager => interactionUIManager;
    void Awake() 
    {
        rb = GetComponent<Rigidbody>();

        inventory = new PlayerInventory();
        faceMouse = new LookObjectToMouse(groundMask);
        playerMovementController = new PlayerMovementController(rb, faceMouse, transform, speed);
        playerInteractionsController = new PlayerInteractions(this, playerMovementController, faceMouse, interactionUIManager, repairCapacity);
        playerAttackController = new PlayerAttackController(spawnPoint, weaponItem, GameObject.FindGameObjectWithTag("Factory").GetComponent<BulletPool>(), this, faceMouse);

        repairAction = InputSystem.actions.FindAction("Attack");
        repairAction.performed += ActiveAttack;
        repairAction.canceled += DeactiveAttack;
        TutorialEvents.OnSetAttackEnabled += SetCanAttack;

        canAttack = true;
    }
    private void Update()
    {
        playerInteractionsController.Update();
        if (canAttack) playerAttackController.Update();
    }
    private void FixedUpdate()
    {
        playerMovementController.FixedUpdate();
    }

    private void OnMove(InputValue value)
    {
        playerMovementController.SetMoveInput(value.Get<Vector2>());
    }

    private void OnInteract()
    {
        GameEvents.InteractConsumed = false;
        GameEvents.InteractPressed();

        if (!GameEvents.InteractConsumed)
            playerInteractionsController.OnInteract();
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

    public void SetCanAttack(bool canAttack)
    {
        this.canAttack = canAttack;
    }
    public void ChangeWeapon(GameObject weapon)
    {
        playerAttackController.SetWeapon(weapon);
    }
    private void OnDestroy()
    {
        playerInteractionsController.Cleanup();
        repairAction.performed -= ActiveAttack;
        repairAction.canceled -= DeactiveAttack;
    }


}
