using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableObjectBrain : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private InteractableType behaviorType;
    private IInteractable objectBehavior;
    InteractInputHandler inputHandler;
    PlayerBrain playerRef;

    void Start()
    {
        objectBehavior = BehaviorFactory.Create(behaviorType);

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
