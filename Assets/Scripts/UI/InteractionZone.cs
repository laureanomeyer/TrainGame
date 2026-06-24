using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    public enum ZoneType { Text, Buttons }

    [SerializeField] private ZoneType zoneType;
    [SerializeField] public string message;
    [SerializeField] private GameObject optionalPanel = null;

    private bool playerInZone;
    private InteractionUIManager ui;
    private PlayerBrain playerBrain;
    private bool isOpen = false;

    private void OnEnable()
    {
        GameEvents.OnInteractPressed += OnPlayerInteract;
    }

    private void OnDestroy()
    {
        GameEvents.OnInteractPressed -= OnPlayerInteract;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!other.TryGetComponent(out PlayerBrain playerBrain)) return;

        this.playerBrain = playerBrain;
        ui = playerBrain.InteractionUIManager;
        playerInZone = true;
        GameEvents.ShowInteract();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInZone = false;
        isOpen = false; 
        ui?.HideAll();
        if (optionalPanel != null)
            optionalPanel.SetActive(false);

        playerBrain = null;
        ui = null;
        GameEvents.HideInteract();
        GameEvents.ShowCursor(true);
    }

    public void DeactivateUI()
    {
        isOpen = false;
        ui?.HideAll();
        if (optionalPanel != null)
            optionalPanel.SetActive(false);

        playerBrain.SetCanMove(true);

        GameEvents.ShowInteract();
        GameEvents.ShowCursor(true);
    }

    private void OnPlayerInteract()
    {
        if (!playerInZone || ui == null) return;

        if (!isOpen)
        {
            isOpen = true;

            string textToShow = TryGetComponent(out WagonShopButton shopButton)
                 ? shopButton.DescriptionText
                 : message;

            if (zoneType == ZoneType.Text)
                ui.ShowText(textToShow);
            else if (zoneType == ZoneType.Buttons)
                ui.ShowButtons();

            if (optionalPanel != null)
                optionalPanel.SetActive(true);

            playerBrain.SetCanMove(false);

            GameEvents.HideInteract();
            GameEvents.ShowCursor(false);
        }
        else
        {
            DeactivateUI();
        }
    }
}