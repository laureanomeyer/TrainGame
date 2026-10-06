using TMPro;
using UnityEngine;

public class ReorderHUD : MonoBehaviour
{
    public enum Option { Move = 0, Upgrade = 1, Sell = 2 }

    [Header("Canvas")]
    [SerializeField] private RectTransform canvasRect;   // RectTransform del Canvas
    [SerializeField] private Camera uiCamera;            // null si el Canvas es Screen Space - Overlay
    [SerializeField] private Camera worldCamera;         // null = Camera.main

    [Header("Sign")]
    [SerializeField] private RectTransform signRoot;     // pivot (0.5, 0): la punta del cartel apunta al vagón
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject optionsRow;
    [SerializeField] private GameObject[] optionHighlights = new GameObject[3]; // marcos blancos: Move, Upgrade, Sell
    [SerializeField] private CanvasGroup upgradeOptionGroup;                     // para atenuar Upgrade
    [SerializeField] private float signWorldOffsetY = 0.5f;
    [SerializeField, Range(0f, 1f)] private float disabledAlpha = 0.4f;

    [Header("Arrows")]
    [SerializeField] private RectTransform leftArrow;
    [SerializeField] private RectTransform rightArrow;
    [SerializeField] private float arrowPadding = 40f;

    [Header("Hints")]
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private string hoverHint = "A/D: Move     F: Select     esc: Cancel";
    [SerializeField] private string panelHint = "F: Select     esc: Cancel";
    [SerializeField] private string dragHint = "A/D: Move     F: Confirm     esc: Cancel";

    [Header("Money colors")]
    [SerializeField] private Color affordColor = new(0.11f, 0.73f, 0.33f);
    [SerializeField] private Color cantAffordColor = new(0.85f, 0.2f, 0.2f);

    private enum Mode { Hidden, Hover, Panel, Drag }
    private Mode mode = Mode.Hidden;
    private ShopWagonData target;

    private static readonly Vector3[] cornersBuffer = new Vector3[4];

    private void Awake()
    {
        ServiceLocator.Register(this);
        ApplyMode(Mode.Hidden);
    }

    private void OnDestroy() => ServiceLocator.Unregister<ReorderHUD>();

    #region API

    public void ShowHover(ShopWagonData wagon, string wagonName, string description)
    {
        target = wagon;
        nameText.text = wagonName;
        descriptionText.text = description;
        ApplyMode(Mode.Hover);
    }

    public void ShowPanel(ShopWagonData wagon, string wagonName, Option selected, string description, bool upgradeEnabled)
    {
        target = wagon;
        nameText.text = wagonName;
        descriptionText.text = description;

        for (int i = 0; i < optionHighlights.Length; i++)
            if (optionHighlights[i] != null) optionHighlights[i].SetActive(i == (int)selected);

        if (upgradeOptionGroup != null)
            upgradeOptionGroup.alpha = upgradeEnabled ? 1f : disabledAlpha;

        ApplyMode(Mode.Panel);
    }

    public void ShowDrag(ShopWagonData wagon)
    {
        target = wagon;
        ApplyMode(Mode.Drag);
    }

    public void Hide()
    {
        target = null;
        ApplyMode(Mode.Hidden);
    }

    public string FormatMoney(float amount, bool affordable)
    {
        string hex = ColorUtility.ToHtmlStringRGB(affordable ? affordColor : cantAffordColor);
        return $"<color=#{hex}>${amount}</color>";
    }

    #endregion

    private void ApplyMode(Mode newMode)
    {
        mode = newMode;

        bool showSign = mode == Mode.Hover || mode == Mode.Panel;
        bool showArrows = mode == Mode.Hover || mode == Mode.Drag;

        signRoot.gameObject.SetActive(showSign);
        optionsRow.SetActive(mode == Mode.Panel);
        leftArrow.gameObject.SetActive(showArrows);
        rightArrow.gameObject.SetActive(showArrows);

        hintText.gameObject.SetActive(mode != Mode.Hidden);
        hintText.text = mode switch
        {
            Mode.Hover => hoverHint,
            Mode.Panel => panelHint,
            Mode.Drag => dragHint,
            _ => string.Empty
        };

        UpdatePositions();
    }

    // LateUpdate: los vagones se mueven con tweens, la UI los sigue cada frame
    private void LateUpdate() => UpdatePositions();

    private void UpdatePositions()
    {
        if (mode == Mode.Hidden || target == null) return;

        Bounds b = target.GetWorldBounds();

        if (mode == Mode.Drag)
        {
            if (TryGetCanvasRect(b, out Rect wagonRect))
                PlaceArrows(wagonRect.center.y, wagonRect.xMin, wagonRect.xMax);
            return;
        }

        Vector3 top = new(b.center.x, b.max.y + signWorldOffsetY, b.center.z);
        if (!WorldToCanvasLocal(top, out Vector2 local)) return;

        signRoot.position = canvasRect.TransformPoint(local);

        if (mode == Mode.Hover)
        {
            Rect signRect = GetCanvasRect(signRoot);
            PlaceArrows(signRect.center.y, signRect.xMin, signRect.xMax);
        }
    }

    private void PlaceArrows(float y, float xMin, float xMax)
    {
        leftArrow.position = canvasRect.TransformPoint(new Vector3(xMin - arrowPadding, y, 0f));
        rightArrow.position = canvasRect.TransformPoint(new Vector3(xMax + arrowPadding, y, 0f));
    }

    #region Conversiones mundo / canvas

    private bool WorldToCanvasLocal(Vector3 world, out Vector2 local)
    {
        var cam = worldCamera != null ? worldCamera : Camera.main;
        Vector3 screen = cam.WorldToScreenPoint(world);

        if (screen.z < 0f) { local = default; return false; } // detrás de la cámara
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, uiCamera, out local);
    }

    // Proyecta los 8 vértices del bounds y devuelve su rectángulo en espacio del canvas
    private bool TryGetCanvasRect(Bounds b, out Rect rect)
    {
        Vector2 min = new(float.MaxValue, float.MaxValue);
        Vector2 max = new(float.MinValue, float.MinValue);
        bool any = false;

        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = new(
                (i & 1) == 0 ? b.min.x : b.max.x,
                (i & 2) == 0 ? b.min.y : b.max.y,
                (i & 4) == 0 ? b.min.z : b.max.z);

            if (!WorldToCanvasLocal(corner, out Vector2 p)) continue;
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
            any = true;
        }

        rect = any ? Rect.MinMaxRect(min.x, min.y, max.x, max.y) : default;
        return any;
    }

    private Rect GetCanvasRect(RectTransform rt)
    {
        rt.GetWorldCorners(cornersBuffer);
        Vector2 min = new(float.MaxValue, float.MaxValue);
        Vector2 max = new(float.MinValue, float.MinValue);

        foreach (var c in cornersBuffer)
        {
            Vector2 p = canvasRect.InverseTransformPoint(c);
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    #endregion
}