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
    public bool IsUsingTurret => isUsingTurret;

    private InputAction repairAction;
    private bool buttonIsHold = false;
    private bool isUsingTurret = false;
    private bool activateOnceParticle = true;
    private bool turretPromptVisible = false;

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

    public void Cleanup()
    {
        if (repairAction != null)
        {
            repairAction.performed -= ActiveRepairInput;
            repairAction.canceled -= DeactiveRepairInput;
        }

        // Si el player se destruye arriba de la torreta, liberarla
        if (usingTurret != null)
        {
            usingTurret.ExitTurret();
            usingTurret = null;
        }
        isUsingTurret = false;

        SetTurretPrompt(false);
    }

    public void Update()
    {
        bool repairingThisFrame = Repair();

        // Corta partículas y devuelve el movimiento si se dejó de reparar por cualquier motivo
        if (!repairingThisFrame)
        {
            if (!activateOnceParticle)
            {
                activateOnceParticle = true;
                playerBrain.sequenceController.StopGroup(ParticleGroups.RepairParticles);
            }

            if (playerBrain.IsRepairing)
            {
                playerBrain.TryRegainMovementAfterRepairing();
            }
        }

        HandleTurretUse();
        RefreshTurretPrompt();
    }

    private bool Repair()
    {
        if (isUsingTurret) return false;
        if (!playerBrain.CanRepair) return false;
        if (!buttonIsHold) return false;
        if (currentWagon == null || !currentWagon.CanBeRepaired) return false;

        currentWagon.Repair(repairCapacity);
        playerBrain.SetIsRepairing(true);
        playerMovementController.SetCanMove(false);
        playerMovementController.SetCanRotate(false);

        if (activateOnceParticle)
        {
            playerBrain.sequenceController.PlayGroup(ParticleGroups.RepairParticles);
            activateOnceParticle = false;
        }

        return true;
    }

    #region Turret prompt (F)
    private void RefreshTurretPrompt()
    {
        bool shouldShow =
            !isUsingTurret &&
            currentTurret != null &&
            currentTurret.CanBeUsed;

        SetTurretPrompt(shouldShow);
    }

    private void SetTurretPrompt(bool show)
    {
        if (turretPromptVisible == show) return;
        turretPromptVisible = show;

        if (show) EventBus.Publish(new OnShowInteractEvent());
        else EventBus.Publish(new OnHideInteractEvent());
    }

    #endregion

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Train"))
        {
            WagonBrain wagon = other.GetComponentInParent<WagonBrain>();
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

            if (!isUsingTurret && currentTurret != null && currentTurret.CanBeUsed && interactionUIManager != null)
            {
                interactionUIManager.ShowWagons("Usar torreta");
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Train"))
        {
            WagonBrain wagon = other.GetComponentInParent<WagonBrain>();
            if (wagon != null && wagon == currentWagon)
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
        if (!turret.EnterTurret()) return;

        usingTurret = turret;
        isUsingTurret = true;

        if (playerMovementController != null)
        {
            playerMovementController.SetCanMove(false);
            playerMovementController.SetCanRotate(false);
            playerMovementController.SetFollowTarget(turret.PlayerUsePoint);
        }

        if (interactionUIManager != null)
        {
            interactionUIManager.ShowWagons("Salir de torreta");
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

        if (playerMovementController != null)
        {
            playerMovementController.SetFollowTarget(null);
        }

        playerBrain.TryRegainMovement();

        if (interactionUIManager != null)
        {
            interactionUIManager.HideAll();
        }
    }

    void HandleTurretUse()
    {
        if (!isUsingTurret) return;


        if (usingTurret == null || usingTurret.IsDisabled)
        {
            StopUsingTurret();
            return;
        }

        if (lookObjectToMouse == null) return;

        Transform aimOrigin = usingTurret.TurretPivot != null
            ? usingTurret.TurretPivot
            : usingTurret.transform;

        Vector3 direction = lookObjectToMouse.GetMouseDirection(aimOrigin);
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        usingTurret.Aim(direction);

        if (playerBrain.CanAttack && playerBrain.PlayerAttackController.IsAttacking)
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
        if (!playerBrain.IsRepairing) return;
        playerBrain.TryRegainMovementAfterRepairing();
    }

    public void OnOpenMainMenu()
    {
        OpenMainMenu();
        EventBus.Publish(new OnHideInteractEvent());
    }

    public void OpenMainMenu()
    {
        GameManager.Instance.GoToMainMenu();
    }
}