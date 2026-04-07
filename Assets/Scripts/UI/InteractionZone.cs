using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    public enum ZoneType
    {
        Text,
        Buttons
    }

    [SerializeField] private ZoneType zoneType;
    [SerializeField] private string message;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        InteractionUIManager ui = FindFirstObjectByType<InteractionUIManager>();

        if (ui == null) return;

        if (zoneType == ZoneType.Text)
        {
            ui.ShowText(message);
        }
        else if (zoneType == ZoneType.Buttons)
        {
            ui.ShowButtons();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        InteractionUIManager ui = FindFirstObjectByType<InteractionUIManager>();

        if (ui == null) return;

        ui.HideAll();
    }
}