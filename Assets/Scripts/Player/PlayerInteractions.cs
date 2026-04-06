using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInteractions : MonoBehaviour
{
    [SerializeField] private WagonBrain currentWagon;
    [SerializeField] private float repairCapacity;


    private PlayerInventory playerInventory;
    private PlayerBrain playerBrain;
    public PlayerInventory Inventory => playerInventory;
    public WagonBrain CurrentWagon => currentWagon;


    private InputAction repairAction;
    private bool buttonIsHold = false;

    void Start()
    {
        playerBrain = GetComponent<PlayerBrain>();
        playerInventory = playerBrain.Inventory;

        repairAction = InputSystem.actions.FindAction("Repair");
        repairAction.performed += ActiveInput;
        repairAction.canceled += DeactiveInput;

    }

    private void Update()
    {
        Repair();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Train"))
        {
            other.TryGetComponent(out WagonBrain wagon);
            currentWagon = wagon;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Train"))
        {
            if (other.TryGetComponent(out WagonBrain wagon) && wagon == currentWagon)
            {
                currentWagon = null;
            }
        }
    }

    public void OnInteract()
    {
        Interact();
    }

    /*
    public void OnRepair()
    {
        Repair();
    }
    */

    void Interact()
    {
        if (currentWagon != null)
        {
            currentWagon.TakeDamage(10);
        }
    }

    public void ActiveInput(InputAction.CallbackContext context)
    {
        buttonIsHold = true;
    }

    public void DeactiveInput(InputAction.CallbackContext context)
    {
        buttonIsHold = false;
    }

    void Repair()
    {
        if (buttonIsHold && currentWagon != null)
        {
            currentWagon.Repair(repairCapacity);
        }
    }
}