using UnityEngine;
using UnityEngine.InputSystem;

public class FuelCharger: MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private LocomotiveBrain locomotive;
    [SerializeField] private BoxCollider coll;
    InteractInputHandler inputHandler;
    PlayerBrain playerRef;

    private bool canInteract = true;

    private void Awake()
    {
        TutorialEvents.OnEnableCoalBox += SetActive;
    }
    void Start()
    {
        inputActions.Enable();
        var interactAction = inputActions.FindAction("Player/Interact");
        inputHandler = new InteractInputHandler(interactAction, OnInteract);
    }

    void OnInteract()
    {
        if (!canInteract) return;
        if (playerRef != null)
            AddFuel();
    }
   
    private void AddFuel()
    {
        if (!canInteract) return;
        if (playerRef.Inventory.HasCoal) 
        {
            locomotive.AddFuel();
            playerRef.Inventory.DepositCoal();
            EventBus.Publish(new OnDropFuelEvent());

            if (GameManager.Instance.CurrentState == GameState.Tutorial) 
            {
                TutorialEvents.SetRunStarted(true);
                TutorialEvents.SetTimerStarted(true);
            }
        }
        else return;
       
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

    void SetActive(bool active)
    {
        canInteract = active;
        coll.enabled = active;
    }
    private void OnDestroy()
    {
        inputHandler.Dispose();
        TutorialEvents.OnEnableCoalBox -= SetActive;
    }



}

