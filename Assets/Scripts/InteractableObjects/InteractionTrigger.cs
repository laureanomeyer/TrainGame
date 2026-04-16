using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    [SerializeField] private PlayerInteractionText playerInteractionText;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (playerInteractionText == null)
        {
            playerInteractionText = other.GetComponent<PlayerInteractionText>();
        }

        if (playerInteractionText != null)
        {
            playerInteractionText.ShowPrompt();
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (playerInteractionText != null)
        {
            playerInteractionText.HidePrompt();
        }
    }
}
