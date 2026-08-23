using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipPanel : MonoBehaviour
{
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private TextMeshProUGUI textToShow;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        ServiceLocator.Register(this);
        gameObject.SetActive(false);
    }
    public void Show(string text, Vector2 screenPosition)
    {
        textToShow.text = text;
        gameObject.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);

        Vector2 calmpedPos = ClampToScreen(screenPosition);
        panelRect.position = calmpedPos;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private Vector2 ClampToScreen(Vector2 desiredPosition)
    {
        Vector2 size = panelRect.rect.size * panelRect.lossyScale;

        float halfWidth = size.x * panelRect.pivot.x;
        float halfHeight = size.y * (1f - panelRect.pivot.y); 

        float minX = size.x * panelRect.pivot.x;
        float maxX = Screen.width - size.x * (1f - panelRect.pivot.x);

        float minY = size.y * panelRect.pivot.y;
        float maxY = Screen.height - size.y * (1f - panelRect.pivot.y);

        float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);

        return new Vector2(clampedX, clampedY);
    }
}
