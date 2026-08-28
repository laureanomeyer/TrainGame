using UnityEngine;
using UnityEngine.EventSystems;

public class HovereableItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string description;
    TooltipPanel tooltipPanelRef;

    [SerializeField] private float showDelay = 0.4f;
    [SerializeField] private float verticalOffset = 20f;

    private RectTransform rectTransform;
    private Coroutine showRoutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanelRef == null)
        {
            ServiceLocator.TryGet<TooltipPanel>(out tooltipPanelRef);
            if (showRoutine != null)
                StopCoroutine(showRoutine);

            showRoutine = StartCoroutine(ShowAfterDelay());
        }
        else if (showRoutine != null) 
        {
            StopCoroutine(showRoutine);

            showRoutine = StartCoroutine(ShowAfterDelay());
        }
        else
        {
            showRoutine = StartCoroutine(ShowAfterDelay());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }

        if (tooltipPanelRef != null)
            tooltipPanelRef.Hide();
    }
    private System.Collections.IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(showDelay);

        Vector2 topPosition = GetTopPosition();
        tooltipPanelRef.Show(description, topPosition);
        showRoutine = null;
    }
    private Vector2 GetTopPosition()
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Vector3 topCenter = (corners[1] + corners[2]) / 2f;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, topCenter);

        return screenPoint + new Vector2(0, verticalOffset);
    }
}
