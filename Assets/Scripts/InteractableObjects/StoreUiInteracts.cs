using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StoreUiInteracts : MonoBehaviour
{
    [SerializeField] private GameObject uiToShow;
    [SerializeField] private GameObject uiContinueConfirmation;
    [SerializeField] private GameObject uiUpgrades;

    [SerializeField] private Button closeButton;

    private PlayerBrain playerBrain;

    private bool playerInZone;
    private bool uiOpen = false;    

    private void OnEnable()
    {
        GameEvents.OnInteractPressed += OnPlayerInteract;
        closeButton.onClick.AddListener(DeactivateUI);
    }

    private void OnDisable()
    {
        GameEvents.OnInteractPressed -= OnPlayerInteract;
        closeButton.onClick.RemoveListener(DeactivateUI);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (playerBrain == null)
        {
            playerBrain = other.gameObject.GetComponent<PlayerBrain>();
        }

        playerInZone = true;

        GameEvents.ShowInteract();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInZone = false;
        playerBrain.SetCanAttack(true);
        uiToShow.SetActive(false);
        uiContinueConfirmation.SetActive(false);
        uiUpgrades.SetActive(false);

        GameEvents.HideInteract();
        GameEvents.ShowCursor(CursorType.Gameplay);
    }

    private void DeactivateUI()
    {
        playerBrain.SetCanAttack(true);
        playerBrain.SetCanMove(true);

        uiToShow.SetActive(false);
        uiContinueConfirmation.SetActive(false);
        uiUpgrades.SetActive(false);

        GameEvents.ShowInteract();
        GameEvents.ShowCursor(CursorType.Gameplay);
    }

    private void OnPlayerInteract()
    {
        if (!playerInZone) return;

        if (!uiOpen)
        {
            uiOpen = true;

            uiToShow.SetActive(true);

            playerBrain.SetCanAttack(false);
            playerBrain.SetCanMove(false);

            GameEvents.ShowCursor(CursorType.Real);
        }
        else
        {
            uiOpen = false;

            DeactivateUI();
        }

        
    }
}
