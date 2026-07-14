using UnityEngine;

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

        EventBus.Publish(new OnShowInteractEvent());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (objectT != null)
            objectT.layer = LayerMask.NameToLayer("Outline");

        EventBus.Publish(new OnHideInteractEvent());
    }
}
