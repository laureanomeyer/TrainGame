using UnityEngine;
using UnityEngine.InputSystem;

public class CollectGoldFromWagon : MonoBehaviour
{
    PlayerBrain playerRef;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GoldenWagonBrain goldBrain;
    InteractInputHandler inputHandler;

    void Start()
    {
        inputActions.Enable();
        var interactAction = inputActions.FindAction("Player/Interact");
        inputHandler = new InteractInputHandler(interactAction, SetGoldInPlayerInventory);
    }

    private void SetGoldInPlayerInventory()
    {
        if (playerRef  != null)
        {
            playerRef.Inventory.GoldAmount = goldBrain.Collector.GiveGold();
            Debug.Log("Oro in player inventory" + playerRef.Inventory.GoldAmount);
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
}
