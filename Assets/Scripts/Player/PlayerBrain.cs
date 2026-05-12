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

    private PlayerInventory inventory;
    private LookObjectToMouse faceMouse;
    private PlayerMovementController playerMovementController;
    private PlayerInteractions playerInteractionsController;
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
    }
    private void Update()
    {
        playerInteractionsController.Update();
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

    private void OnDestroy()
    {
        playerInteractionsController.Cleanup();
    }
}
