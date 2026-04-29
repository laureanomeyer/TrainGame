using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInteractions : MonoBehaviour
{
    [SerializeField] private WagonBrain currentWagon;
    [SerializeField] private float repairCapacity;
    [SerializeField] private InteractionUIManager interactionUIManager;

    private ShopButton currentButton;
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

    void Start()
    {
        playerBrain = GetComponent<PlayerBrain>();
        playerMovementController = GetComponent<PlayerMovementController>();
        lookObjectToMouse = GetComponent<LookObjectToMouse>();

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
        HandleTurretUse();
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
                interactionUIManager.ShowText(currentButton.DescriptionText);
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
    }

    void Repair()
    {
        if (isUsingTurret) return;

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

    public void OnOpenMainMenu()
    {
        OpenMainMenu();
    }

    public void OpenMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}