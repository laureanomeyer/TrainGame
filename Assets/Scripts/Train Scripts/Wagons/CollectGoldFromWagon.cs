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
        TutorialEvents.OnEnemyKilled += Activate;
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
        TutorialEvents.OnEnemyKilled -= Activate;
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

    void Activate()
    {
        canInteract = true;
        boxCollider.enabled = true;
    }
}
