using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractions
{
    private float repairCapacity;
    private InteractionUIManager interactionUIManager;

    private WagonBrain currentWagon;
    private WagonShopButton currentButton;
    private ShopZone currentShopZone;
    private ActiveTurretStation currentTurretStation;
    private ActiveTurretStation usingTurretStation;

    private PlayerInventory playerInventory;
    private PlayerBrain playerBrain;
    private PlayerMovementController playerMovementController;
    private LookObjectToMouse lookObjectToMouse;

    public PlayerInventory Inventory => playerInventory;
    public WagonBrain CurrentWagon => currentWagon;

    private InputAction repairAction;
    private bool buttonIsHold = false;
    private bool isUsingTurret = false;

    public PlayerInteractions(PlayerBrain playerBrain, PlayerMovementController playerMovementController, LookObjectToMouse lookObjectToMouse, InteractionUIManager interactionUIManager, float repairCapacity)
    {
        this.playerBrain = playerBrain;
        this.playerMovementController = playerMovementController;
        this.lookObjectToMouse = lookObjectToMouse;
        playerInventory = playerBrain.Inventory;
        repairAction = InputSystem.actions.FindAction("Repair");
        repairAction.performed += ActiveRepairInput;
        repairAction.canceled += DeactiveRepairInput;
        this.interactionUIManager = interactionUIManager;
        this.repairCapacity = repairCapacity;

        if (interactionUIManager != null)
        {
            interactionUIManager.HideAll();
        }
    }

    public void Update()
    {
        Repair();
        HandleTurretUse();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Train"))
        {
            WagonBrain wagon = other.GetComponentInParent<WagonBrain>();

            if(wagon != null)
            {
                if (currentWagon != null)
                {
                    currentWagon.HideHpBar();
                }
                currentWagon = wagon;
                currentWagon.ShowHpBar();
            }
        }

        if (other.CompareTag("ShopButton"))
        {
            other.TryGetComponent(out WagonShopButton shopButton);
            currentButton = shopButton;
        }

        if (other.CompareTag("ShopZone"))
        {
            other.TryGetComponent(out ShopZone shopZone);
            currentShopZone = shopZone;
        }

        if (other.CompareTag("ActiveTurret"))
        {
            other.TryGetComponent(out ActiveTurretStation turretStation);
            currentTurretStation = turretStation;

            if (!isUsingTurret && currentTurretStation != null && interactionUIManager != null)
            {
                interactionUIManager.ShowText("Usar torreta");
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Train"))
        {
            if (other.TryGetComponent(out WagonBrain wagon) && wagon == currentWagon)
            {
                currentWagon.HideHpBar();
                currentWagon = null;
            }  
        }

        if (other.CompareTag("ShopButton"))
        {
            if (other.TryGetComponent(out WagonShopButton shopButton) && shopButton == currentButton)
            {
                currentButton = null;

                if (!isUsingTurret && interactionUIManager != null)
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

                if (!isUsingTurret && interactionUIManager != null)
                {
                    interactionUIManager.HideAll();
                }
            }
        }

        if (other.CompareTag("ActiveTurret"))
        {
            if (other.TryGetComponent(out ActiveTurretStation turretStation) && turretStation == currentTurretStation)
            {
                currentTurretStation = null;

                if (!isUsingTurret && interactionUIManager != null)
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
        if (isUsingTurret)
        {
            StopUsingTurret();
            return;
        }

        if (currentTurretStation != null)
        {
            StartUsingTurret(currentTurretStation);
            return;
        }

        if (currentButton != null)
        {
            currentButton.Interact();
        }
    }

    void StartUsingTurret(ActiveTurretStation turretStation)
    {
        usingTurretStation = turretStation;
        isUsingTurret = true;

        if (playerMovementController != null)
        {
            playerMovementController.SetCanMove(false);
            playerMovementController.SetCanRotate(false);
        }

        if (interactionUIManager != null)
        {
            interactionUIManager.ShowText("Salir de torreta");
        }
    }

    void StopUsingTurret()
    {
        isUsingTurret = false;
        usingTurretStation = null;

        if (playerMovementController != null)
        {
            playerMovementController.SetCanMove(true);
            playerMovementController.SetCanRotate(true);
        }

        if (interactionUIManager != null)
        {
            interactionUIManager.HideAll();
        }
    }

    void HandleTurretUse()
    {
        if (!isUsingTurret) return;
        if (usingTurretStation == null) return;
        if (usingTurretStation.Turret == null) return;
        if (lookObjectToMouse == null) return;

        Transform aimOrigin = usingTurretStation.Turret.FirePoint != null
            ? usingTurretStation.Turret.FirePoint
            : usingTurretStation.Turret.transform;

        if (Mouse.current.leftButton.isPressed)
        {
            Vector3 direction = lookObjectToMouse.GetMouseDirection(aimOrigin);
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                usingTurretStation.Turret.TryShoot(direction);
            }
        }
    }

    public void ActiveRepairInput(InputAction.CallbackContext context)
    {
        buttonIsHold = true;
    }

    public void DeactiveRepairInput(InputAction.CallbackContext context)
    {
        buttonIsHold = false;
        playerBrain.SetIsRepairing(false);
        playerMovementController.SetCanMove(true);
    }

    void Repair()
    {
        if (isUsingTurret) return;

        if (buttonIsHold && currentWagon != null && currentWagon.CanBeRepaired)
        {
            currentWagon.Repair(repairCapacity);
            playerBrain.SetIsRepairing(true);
            playerMovementController.SetCanMove(false);
        }
    }

    public void OnOpenMainMenu()
    {
        OpenMainMenu();
        GameEvents.HideInteract();
    }

    public void OpenMainMenu()
    {
        GameManager.Instance.EndSession();
    }

    public void Cleanup()
    {
        if (repairAction != null)
        {
            repairAction.performed -= ActiveRepairInput;
            repairAction.canceled -= DeactiveRepairInput;
        }
    }
}