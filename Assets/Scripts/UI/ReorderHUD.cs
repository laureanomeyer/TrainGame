using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    [Tooltip("Off: el cartel queda fijo donde lo pongas en el Canvas. On: sigue la altura del vagón.")]
    [SerializeField] private bool signFollowsWagon = false;

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

    [Header("Clickable")]
    [SerializeField] private Button moveButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    [Tooltip("Opcional: equivale a F (Select / Confirm).")]
    [SerializeField] private Button confirmButton;
    [Tooltip("Opcional: equivale a esc (Cancel).")]
    [SerializeField] private Button cancelButton;

    public event Action<Option> OptionHovered;
    public event Action<Option> OptionClicked;
    public event Action<Option> OptionUnhovered;
    public event Action<int> ArrowClicked;  
    public event Action ConfirmClicked;
    public event Action CancelClicked;

    private enum Mode { Hidden, Hover, Panel, Drag }
    private Mode mode = Mode.Hidden;
    private ShopWagonData target;


    private CanvasGroup signGroup;
    private CanvasGroup nameGroup;
    private LayoutElement nameLayout;
    private CanvasGroup optionsGroup;
    private CanvasGroup leftArrowGroup;
    private CanvasGroup rightArrowGroup;
    private CanvasGroup hintGroup;
    private CanvasGroup[] highlightGroups;

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

    #region Lifecycle

    private void Awake()
    {
        ServiceLocator.Register(this);

        affordHex = ColorUtility.ToHtmlStringRGB(affordColor);
        cantAffordHex = ColorUtility.ToHtmlStringRGB(cantAffordColor);

        CacheGroups();
        WireButtons();

        ApplyMode(Mode.Hidden);
    }

    private void OnDestroy() => ServiceLocator.Unregister<ReorderHUD>();

    private void CacheGroups()
    {
        signGroup = GetOrAddGroup(signRoot.gameObject);
        optionsGroup = GetOrAddGroup(optionsRow);
        leftArrowGroup = GetOrAddGroup(leftArrow.gameObject);
        rightArrowGroup = GetOrAddGroup(rightArrow.gameObject);
        hintGroup = GetOrAddGroup(hintText.gameObject);

        GameObject nameGo = nameRoot != null ? nameRoot : nameText.gameObject;
        nameGroup = GetOrAddGroup(nameGo);
        nameLayout = nameGo.GetComponent<LayoutElement>() ?? nameGo.AddComponent<LayoutElement>();

        highlightGroups = new CanvasGroup[optionHighlights.Length];
        for (int i = 0; i < optionHighlights.Length; i++)
            highlightGroups[i] = optionHighlights[i] != null ? GetOrAddGroup(optionHighlights[i]) : null;
    }

    private static CanvasGroup GetOrAddGroup(GameObject go)
    {
        if (go == null) return null;
        var g = go.GetComponent<CanvasGroup>();
        return g != null ? g : go.AddComponent<CanvasGroup>();
    }

    #endregion

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

    public void PreviewOption(string wagonName, Option option, string description, bool upgradeEnabled)
    {
        SetName(option == Option.Upgrade ? null : wagonName);
        SetTextIfChanged(descriptionText, description);
        SetOptions((int)option, upgradeEnabled);

        forcedRefreshFrames = 2; 
    }

    #endregion

    #region Modo y visibilidad

    private void ApplyMode(Mode newMode)
    {
        mode = newMode;

        bool showSign = mode == Mode.Hover || mode == Mode.Panel;
        bool showArrows = mode == Mode.Hover || mode == Mode.Drag;

        SetVisible(signGroup, showSign);
        SetVisible(optionsGroup, showSign);
        SetVisible(leftArrowGroup, showArrows);
        SetVisible(rightArrowGroup, showArrows);
        SetVisible(hintGroup, mode != Mode.Hidden);

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


    private void SetOptions(int selectedIndex, bool upgradeEnabled)
    {
        for (int i = 0; i < highlightGroups.Length; i++)
            SetAlpha(highlightGroups[i], i == selectedIndex ? 1f : 0f);


        SetAlpha(upgradeOptionGroup, upgradeEnabled ? 1f : disabledAlpha);
    }


    private void SetName(string wagonName)
    {
        bool show = !string.IsNullOrEmpty(wagonName);

        SetVisible(nameGroup, show);
        if (nameLayout.ignoreLayout == show) nameLayout.ignoreLayout = !show;

        if (show) SetTextIfChanged(nameText, wagonName);
    }

    #endregion

    #region Posicionamiento

    private void LateUpdate()
    {
        if (mode == Mode.Hidden || target == null) return;

        bool forced = forcedRefreshFrames > 0;
        if (forced) forcedRefreshFrames--;


        bool tracksWorld = mode == Mode.Drag || signFollowsWagon;

        if (!tracksWorld)
        {
            if (forced || ScreenSizeChanged()) UpdatePositions();
            return;
        }

        Camera cam = GetWorldCamera();
        if (cam == null) return;

        if (!forced && !HasViewChanged(cam)) return;

        UpdatePositions();
    }

    private bool ScreenSizeChanged()
    {
        if (Screen.width == lastScreenW && Screen.height == lastScreenH) return false;

        lastScreenW = Screen.width;
        lastScreenH = Screen.height;
        return true;
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


        if (mode == Mode.Drag)
        {
            Camera cam = GetWorldCamera();
            if (cam != null && TryGetCanvasRect(cam, target.GetWorldBounds(), out Rect wagonRect))
                PlaceArrows(wagonRect.center.y, wagonRect.xMin, wagonRect.xMax);
            return;
        }


        if (signFollowsWagon)
        {
            Camera cam = GetWorldCamera();
            if (cam == null) return;

            Bounds b = target.GetWorldBounds();
            Vector3 top = new(b.center.x, b.max.y + signWorldOffsetY, b.center.z);
            if (!WorldToCanvasLocal(cam, top, out Vector2 local)) return;

            SetPositionIfChanged(signRoot, canvasRect.TransformPoint(local));
        }


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

    #endregion

    #region Clicks

    private void WireButtons()
    {
        WireOption(moveButton, Option.Move);
        WireOption(upgradeButton, Option.Upgrade);
        WireOption(sellButton, Option.Sell);
        WireClick(leftArrowButton, () => ArrowClicked?.Invoke(-1));
        WireClick(rightArrowButton, () => ArrowClicked?.Invoke(+1));
        WireClick(confirmButton, () => ConfirmClicked?.Invoke());
        WireClick(cancelButton, () => CancelClicked?.Invoke());
    }

    private void WireOption(Button button, Option option)
    {
        if (button == null) return;

        WireClick(button, () => OptionClicked?.Invoke(option));


        var trigger = button.GetComponent<EventTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();

        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => OptionHovered?.Invoke(option));
        trigger.triggers.Add(enter);

        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => OptionUnhovered?.Invoke(option));
        trigger.triggers.Add(exit);
    }

    private static void WireClick(Button button, Action action)
    {
        if (button == null) return;


        button.navigation = new Navigation { mode = Navigation.Mode.None };

        button.onClick.AddListener(() =>
        {
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            action();
        });
    }

    #endregion

    #region Helpers sin escrituras innecesarias


    private static void SetVisible(CanvasGroup g, bool visible)
    {
        if (g == null) return;

        float a = visible ? 1f : 0f;
        if (g.alpha != a) g.alpha = a;
        if (g.blocksRaycasts != visible) g.blocksRaycasts = visible;
        if (g.interactable != visible) g.interactable = visible;
    }

    private static void SetAlpha(CanvasGroup g, float alpha)
    {
        if (g != null && !Mathf.Approximately(g.alpha, alpha)) g.alpha = alpha;
    }

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