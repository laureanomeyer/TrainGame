
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractions : MonoBehaviour
{
    [SerializeField] private WagonBrain currentWagon;
    [SerializeField] private float repairCapacity;
    [SerializeField] private InteractionUIManager interactionUIManager;

    private ShopButton currentButton;
    private ShopZone currentShopZone;

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
        repairAction.performed += ActiveRepairInput;
        repairAction.canceled += DeactiveRepairInput;

        if (interactionUIManager != null)
        {
            interactionUIManager.HideAll();
        }
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

        if (other.CompareTag("ShopButton"))
        {
            other.TryGetComponent(out ShopButton shopButton);
            currentButton = shopButton;

            if (currentButton != null && interactionUIManager != null)
            {
                interactionUIManager.ShowText(currentButton.ButtonText);
            }
        }

        if (other.CompareTag("ShopZone"))
        {
            other.TryGetComponent(out ShopZone shopZone);
            currentShopZone = shopZone;

            if (currentShopZone != null && interactionUIManager != null)
            {
                interactionUIManager.ShowButtons();
            }
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

        if (other.CompareTag("ShopButton"))
        {
            if (other.TryGetComponent(out ShopButton shopButton) && shopButton == currentButton)
            {
                currentButton = null;

                if (interactionUIManager != null)
                {
                    interactionUIManager.HideAll();
                }
            }
        }

        if (other.CompareTag("ShopZone"))
        {
            if (other.TryGetComponent(out ShopZone shopZone) && shopZone == currentShopZone)
            {
                currentShopZone = null;

                if (interactionUIManager != null)
                {
                    interactionUIManager.HideAll();
                }
            }
        }
    }

    public void OnInteract()
    {
        Interact();
    }

    void Interact()
    {
        if (currentButton != null)
        {
            currentButton.Interact();
        }
    }

    public void ActiveRepairInput(InputAction.CallbackContext context)
    {
        buttonIsHold = true;
    }

    public void DeactiveRepairInput(InputAction.CallbackContext context)
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
    public void OnReloadScene()
    {
        ReloadScene();
    }
    public void ReloadScene()
    {
        GameManager.Instance.GoToRun();
    }
}