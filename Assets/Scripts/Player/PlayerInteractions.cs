using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractions
{
    private readonly float repairCapacity;
    private InteractionUIManager interactionUIManager;

    private WagonBrain currentWagon;
    private WagonShopButton currentButton;
    private ShopZone currentShopZone;

    private WagonTurret currentTurret;
    private WagonTurret usingTurret;

    private PlayerInventory playerInventory;
    private PlayerBrain playerBrain;
    private PlayerMovementController playerMovementController;
    private LookObjectToMouse lookObjectToMouse;

    public PlayerInventory Inventory => playerInventory;
    public WagonBrain CurrentWagon => currentWagon;

    private InputAction repairAction;
    private bool buttonIsHold = false;
    private bool isUsingTurret = false;

    public PlayerInteractions (PlayerBrain playerBrain, PlayerMovementController playerMovementController, LookObjectToMouse lookObjectToMouse, InteractionUIManager interactionUIManager, float repairCapacity)
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
        WagonBrain wagon = other.GetComponentInParent<WagonBrain>();
        if (other.CompareTag("Train"))
        {
            if (wagon != null)
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
            currentTurret = other.GetComponentInParent<WagonTurret>();

            if (!isUsingTurret && currentTurret != null && interactionUIManager != null)
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
            WagonTurret turret = other.GetComponentInParent<WagonTurret>();

            if (turret != null && turret == currentTurret)
            {
                currentTurret = null;

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

        if (currentTurret != null)
        {
            StartUsingTurret(currentTurret);
            return;
        }
    }

    void StartUsingTurret(WagonTurret turret)
    {
        if (turret == null) return;
        if (turret.IsOccupied) return;

        usingTurret = turret;
        isUsingTurret = true;

        turret.EnterTurret();

        playerBrain.SetCanAttack(false);

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
        if (usingTurret != null)
        {
            usingTurret.ExitTurret();
        }

        isUsingTurret = false;
        usingTurret = null;

        playerBrain.SetCanAttack(true);

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
        if (usingTurret == null) return;
        if (lookObjectToMouse == null) return;

        Transform aimOrigin = usingTurret.FirePoint != null
            ? usingTurret.FirePoint
            : usingTurret.transform;

        Vector3 direction = lookObjectToMouse.GetMouseDirection(aimOrigin);
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        // La torreta rota siempre hacia el mouse.
        usingTurret.Aim(direction);

        // Solo dispara mientras mantenés click.
        if (Mouse.current.leftButton.isPressed)
        {
            usingTurret.TryShoot(direction);
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
        playerMovementController.SetCanRotate(true);
    }

    void Repair()
    {
        if (isUsingTurret) return;

        if (buttonIsHold && currentWagon != null && currentWagon.CanBeRepaired)
        {
            currentWagon.Repair(repairCapacity);
            playerBrain.SetIsRepairing(true);
            playerMovementController.SetCanMove(false);
            playerMovementController.SetCanRotate(false);
        }
    }

    public void OnOpenMainMenu()
    {
        OpenMainMenu();
        GameEvents.HideInteract();
    }

    public void OpenMainMenu()
    {
        GameManager.Instance.GoToMainMenu();
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