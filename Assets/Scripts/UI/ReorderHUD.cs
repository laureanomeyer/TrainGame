using TMPro;
using UnityEngine;

public class ReorderHUD : MonoBehaviour
{
    public enum Option { Move = 0, Upgrade = 1, Sell = 2 }

    [Header("Canvas")]
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Camera worldCamera;

    [Header("Sign")]
    [SerializeField] private RectTransform signRoot;
    [SerializeField] private TextMeshProUGUI nameText;
    [Tooltip("Objeto que se oculta junto con el nombre (ej: NameBox). Si está vacío, se oculta solo el texto.")]
    [SerializeField] private GameObject nameRoot;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject optionsRow;
    [SerializeField] private GameObject[] optionHighlights = new GameObject[3];
    [SerializeField] private CanvasGroup upgradeOptionGroup;
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
    [SerializeField] private CanvasGroup hintTexts;

    [Header("Money colors")]
    [SerializeField] private Color affordColor = new(0.11f, 0.73f, 0.33f);
    [SerializeField] private Color cantAffordColor = new(0.85f, 0.2f, 0.2f);

    private enum Mode { Hidden, Hover, Panel, Drag }
    private Mode mode = Mode.Hidden;
    private ShopWagonData target;

    private static readonly Vector3[] cornersBuffer = new Vector3[4];

    private const float PosEpsilonSqr = 0.000001f;
    private const float RotDotEpsilon = 0.999999f;

    private int forcedRefreshFrames;

    private Transform lastTargetTransform;
    private Vector3 lastTargetPos;
    private Quaternion lastTargetRot;
    private Vector3 lastTargetScale;
    private Vector3 lastCamPos;
    private Quaternion lastCamRot;
    private float lastCamFov;
    private int lastScreenW;
    private int lastScreenH;

    private Camera cachedMainCamera;

    private string affordHex;
    private string cantAffordHex;

    private void Awake()
    {
        ServiceLocator.Register(this);

        affordHex = ColorUtility.ToHtmlStringRGB(affordColor);
        cantAffordHex = ColorUtility.ToHtmlStringRGB(cantAffordColor);

        ApplyMode(Mode.Hidden);
    }

    private void OnDestroy() => ServiceLocator.Unregister<ReorderHUD>();

    #region API

    public void ShowHover(ShopWagonData wagon, string wagonName, string description, bool upgradeEnabled)
    {
        target = wagon;
        SetName(wagonName);
        SetTextIfChanged(descriptionText, description);
        SetOptions(-1, upgradeEnabled);
        ApplyMode(Mode.Hover);
    }

    public void ShowPanel(ShopWagonData wagon, string wagonName, Option selected, string description, bool upgradeEnabled)
    {
        target = wagon;


        SetName(selected == Option.Upgrade ? null : wagonName);
        SetTextIfChanged(descriptionText, description);
        SetOptions((int)selected, upgradeEnabled);

        ApplyMode(Mode.Panel);
    }


    private void SetOptions(int selectedIndex, bool upgradeEnabled)
    {
        for (int i = 0; i < optionHighlights.Length; i++)
            SetActiveIfChanged(optionHighlights[i], i == selectedIndex);

        if (upgradeOptionGroup != null)
        {
            float alpha = upgradeEnabled ? 1f : disabledAlpha;
            if (!Mathf.Approximately(upgradeOptionGroup.alpha, alpha)) upgradeOptionGroup.alpha = alpha;
        }
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
        return "<color=#" + (affordable ? affordHex : cantAffordHex) + ">$" + amount + "</color>";
    }

    #endregion

    private void ApplyMode(Mode newMode)
    {
        mode = newMode;

        bool showSign = mode == Mode.Hover || mode == Mode.Panel;
        bool showArrows = mode == Mode.Hover || mode == Mode.Drag;

        SetActiveIfChanged(signRoot.gameObject, showSign);
        SetActiveIfChanged(optionsRow, mode == Mode.Hover || mode == Mode.Panel);
        SetActiveIfChanged(leftArrow.gameObject, showArrows);
        SetActiveIfChanged(rightArrow.gameObject, showArrows);
        SetActiveIfChanged(hintText.gameObject, mode != Mode.Hidden);

        SetTextIfChanged(hintText, mode switch
        {
            Mode.Hover => hoverHint,
            Mode.Panel => panelHint,
            Mode.Drag => dragHint,
            _ => string.Empty
        });

        forcedRefreshFrames = 2;
        UpdatePositions();
    }

    private void LateUpdate()
    {
        if (mode == Mode.Hidden || target == null) return;

        Camera cam = GetWorldCamera();
        if (cam == null) return;

        bool forced = forcedRefreshFrames > 0;
        if (forced) forcedRefreshFrames--;

        if (!forced && !HasViewChanged(cam)) return;

        UpdatePositions();
    }

    private Camera GetWorldCamera()
    {
        if (worldCamera != null) return worldCamera;
        if (cachedMainCamera == null) cachedMainCamera = Camera.main;
        return cachedMainCamera;
    }

    private bool HasViewChanged(Camera cam)
    {
        Transform t = target.transform;
        Transform camT = cam.transform;

        t.GetPositionAndRotation(out Vector3 pos, out Quaternion rot);
        Vector3 scale = t.lossyScale;
        camT.GetPositionAndRotation(out Vector3 camPos, out Quaternion camRot);
        float fov = cam.fieldOfView;

        bool changed =
            t != lastTargetTransform ||
            (pos - lastTargetPos).sqrMagnitude > PosEpsilonSqr ||
            (scale - lastTargetScale).sqrMagnitude > PosEpsilonSqr ||
            Mathf.Abs(Quaternion.Dot(rot, lastTargetRot)) < RotDotEpsilon ||
            (camPos - lastCamPos).sqrMagnitude > PosEpsilonSqr ||
            Mathf.Abs(Quaternion.Dot(camRot, lastCamRot)) < RotDotEpsilon ||
            !Mathf.Approximately(fov, lastCamFov) ||
            Screen.width != lastScreenW ||
            Screen.height != lastScreenH;

        if (changed)
        {
            lastTargetTransform = t;
            lastTargetPos = pos;
            lastTargetRot = rot;
            lastTargetScale = scale;
            lastCamPos = camPos;
            lastCamRot = camRot;
            lastCamFov = fov;
            lastScreenW = Screen.width;
            lastScreenH = Screen.height;
        }

        return changed;
    }

    private void UpdatePositions()
    {
        if (mode == Mode.Hidden || target == null) return;

        Camera cam = GetWorldCamera();
        if (cam == null) return;

        Bounds b = target.GetWorldBounds();

        if (mode == Mode.Drag)
        {
            if (TryGetCanvasRect(cam, b, out Rect wagonRect))
                PlaceArrows(wagonRect.center.y, wagonRect.xMin, wagonRect.xMax);
            return;
        }

        Vector3 top = new(b.center.x, b.max.y + signWorldOffsetY, b.center.z);
        if (!WorldToCanvasLocal(cam, top, out Vector2 local)) return;

        SetPositionIfChanged(signRoot, canvasRect.TransformPoint(local));

        if (mode == Mode.Hover)
        {
            Rect signRect = GetCanvasRect(signRoot);
            PlaceArrows(signRect.center.y, signRect.xMin, signRect.xMax);
        }
    }

    private void PlaceArrows(float y, float xMin, float xMax)
    {
        SetPositionIfChanged(leftArrow, canvasRect.TransformPoint(new Vector3(xMin - arrowPadding, y, 0f)));
        SetPositionIfChanged(rightArrow, canvasRect.TransformPoint(new Vector3(xMax + arrowPadding, y, 0f)));
    }

    // null o vacío → oculta el nombre (y su caja si nameRoot está asignado)
    private void SetName(string wagonName)
    {
        bool show = !string.IsNullOrEmpty(wagonName);
        GameObject root = nameRoot != null ? nameRoot : nameText.gameObject;

        SetActiveIfChanged(root, show);
        if (show) SetTextIfChanged(nameText, wagonName);
    }

    #region Helpers sin escrituras innecesarias

    private static void SetPositionIfChanged(RectTransform rt, Vector3 worldPos)
    {
        if ((rt.position - worldPos).sqrMagnitude > PosEpsilonSqr)
            rt.position = worldPos;
    }

    private static void SetTextIfChanged(TMP_Text text, string value)
    {
        if (text == null) return;
        value ??= string.Empty;
        if (text.text != value) text.text = value;
    }

    private static void SetActiveIfChanged(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active) go.SetActive(active);
    }

    #endregion

    #region Conversiones mundo / canvas

    private bool WorldToCanvasLocal(Camera cam, Vector3 world, out Vector2 local)
    {
        Vector3 screen = cam.WorldToScreenPoint(world);

        if (screen.z < 0f) { local = default; return false; }
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, uiCamera, out local);
    }

    private bool TryGetCanvasRect(Camera cam, Bounds b, out Rect rect)
    {
        Vector2 min = new(float.MaxValue, float.MaxValue);
        Vector2 max = new(float.MinValue, float.MinValue);
        bool any = false;

        Vector3 bMin = b.min;
        Vector3 bMax = b.max;

        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = new(
                (i & 1) == 0 ? bMin.x : bMax.x,
                (i & 2) == 0 ? bMin.y : bMax.y,
                (i & 4) == 0 ? bMin.z : bMax.z);

            if (!WorldToCanvasLocal(cam, corner, out Vector2 p)) continue;
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

        for (int i = 0; i < cornersBuffer.Length; i++)
        {
            Vector2 p = canvasRect.InverseTransformPoint(cornersBuffer[i]);
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    #endregion
}