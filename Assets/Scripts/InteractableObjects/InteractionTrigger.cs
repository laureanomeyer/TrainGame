using UnityEngine;
using UnityEngine.UI;

public class InteractionTrigger : MonoBehaviour
{
    [SerializeField] private GameObject outline;

    private void Awake()
    {
        outline.SetActive(false);
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (outline != null) outline.SetActive(true);       
        GameEvents.ShowInteract();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (outline != null) outline.SetActive(false);
        GameEvents.HideInteract();

    }
}
