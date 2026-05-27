using UnityEngine;
using UnityEngine.InputSystem;

public class StoreUiInteracts : MonoBehaviour
{
    [SerializeField] private GameObject uiToShow;
    [SerializeField] private GameObject uiContinueConfirmation;
    [SerializeField] private GameObject uiUpgrades;
    private PlayerBrain playerBrain;

    private bool playerInZone;

    private void OnEnable()
    {
        GameEvents.OnInteractPressed += OnPlayerInteract;
    }

    private void OnDisable()
    {
        GameEvents.OnInteractPressed -= OnPlayerInteract;
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
        GameEvents.HideInteract();
        playerBrain.SetCanAttack(true);
        uiToShow.SetActive(false);
        uiContinueConfirmation.SetActive(false);
        uiUpgrades.SetActive(false);
    }

    private void OnPlayerInteract()
    {
        if (!playerInZone) return;
        uiToShow.SetActive(true);
        playerBrain.SetCanAttack(false);
    }
}
