using UnityEngine;
using UnityEngine.UI;

public class InteractionTrigger : MonoBehaviour
{
    [SerializeField] private GameObject objectT;
    [SerializeField] private LayerMask outlineLayer;
    [SerializeField] private LayerMask whiteOutlineLayer;

    private void Awake()
    {
        if (objectT != null)
            objectT.layer = LayerMask.NameToLayer("Outline");   
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (objectT != null)
            objectT.layer = LayerMask.NameToLayer("WhiteOutline");   
        
        GameEvents.ShowInteract();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (objectT != null)
            objectT.layer = LayerMask.NameToLayer("Outline");

        GameEvents.HideInteract();
    }
}
