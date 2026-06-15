using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableObjectBrain : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private InteractableType behaviorType;
    [SerializeField] private BoxCollider col;
    private IInteractableWithInventory objectBehavior;
    InteractInputHandler inputHandler;
    PlayerBrain playerRef;

    void Awake()
    {
        objectBehavior = BehaviorFactory.Create(behaviorType, col);

        inputActions.Enable();
        var interactAction = inputActions.FindAction("Player/Interact");
        inputHandler = new InteractInputHandler(interactAction, OnInteract);
    }

    void OnInteract()
    {
        if (playerRef != null)
        {
            objectBehavior.Interact(playerRef.Inventory);
        } 
    }
    private void OnDestroy()
    {
        inputHandler.Dispose();
        objectBehavior.OnDestroyObject();
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

}
