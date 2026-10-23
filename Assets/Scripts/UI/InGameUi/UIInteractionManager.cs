using DG.Tweening;
using TMPro;
using UnityEngine;

public class InteractionUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TMP_Text spacialText;
    [SerializeField] private GameObject wagonsPanel;
    [SerializeField] private GameObject weaponsPanel;
    [SerializeField] private GameObject UpgradesObject;
    [SerializeField] private StationRouteSO stationRoute;

    [Header("Wagons Panel Animation (slide from right)")]
    [SerializeField] private float wagonsSlideDuration = 0.4f;
    [Tooltip("Distancia hacia la derecha desde donde arranca. Tiene que alcanzar para quedar fuera de pantalla.")]
    [SerializeField] private float wagonsSlideOffset = 1500f;
    [SerializeField] private Ease wagonsSlideEase = Ease.OutBack;

    [Header("Weapons Panel Animation (slide from left)")]
    [SerializeField] private float weaponsSlideDuration = 0.4f;
    [Tooltip("Distancia hacia la izquierda desde donde arranca. Tiene que alcanzar para quedar fuera de pantalla.")]
    [SerializeField] private float weaponsSlideOffset = 1500f;
    [SerializeField] private Ease weaponsSlideEase = Ease.OutBack;

    private RectTransform wagonsRect;
    private RectTransform weaponsRect;

    private Vector2 wagonsOriginalPos;
    private Vector2 weaponsOriginalPos;

    private Tween wagonsTween;
    private Tween weaponsTween;

    private void Awake()
    {
        wagonsRect = wagonsPanel.GetComponent<RectTransform>();
        weaponsRect = weaponsPanel.GetComponent<RectTransform>();

        wagonsOriginalPos = wagonsRect.anchoredPosition;
        weaponsOriginalPos = weaponsRect.anchoredPosition;
    }

    private void Start()
    {
        HideAll();

        var sessionConfigRef = ServiceLocator.Get<SessionConfig>();

        string stationName = stationRoute != null ? stationRoute.GetStationNameByLevel(sessionConfigRef.CurrentLevel) : "Unknown Station";

        spacialText.text = stationName;

        UpgradesObject.SetActive(true);

        EventBus.Publish(new OnActivateUiEvent(false));
        EventBus.Publish(new OnShowCursorEvent(CursorType.Real));
        GameManager.Instance.ChangeGameState(GameState.UI);
    }

    public void ShowWagons(string message)
    {
        HideWeaponsPanel();

        infoText.text = message;

        if (wagonsPanel.activeSelf)
            return;

        wagonsTween.Kill();

        wagonsRect.anchoredPosition = wagonsOriginalPos + Vector2.right * wagonsSlideOffset;
        wagonsPanel.SetActive(true);

        wagonsTween = wagonsRect.DOAnchorPos(wagonsOriginalPos, wagonsSlideDuration).SetEase(wagonsSlideEase).SetUpdate(true).SetLink(gameObject);
    }

    public void ShowWeapons()
    {
        HideWagonsPanel();

        if (weaponsPanel.activeSelf)
            return;

        weaponsTween.Kill();

        weaponsRect.anchoredPosition = weaponsOriginalPos + Vector2.left * weaponsSlideOffset;
        weaponsPanel.SetActive(true);

        weaponsTween = weaponsRect.DOAnchorPos(weaponsOriginalPos, weaponsSlideDuration).SetEase(weaponsSlideEase).SetUpdate(true).SetLink(gameObject);
    }

    public void HideAll()
    {
        if (wagonsRect == null || weaponsRect == null || wagonsOriginalPos == null || weaponsOriginalPos == null) return;
        HideWagonsPanel();
        HideWeaponsPanel();
    }

    private void HideWagonsPanel()
    {
        wagonsTween.Kill();
        wagonsRect.anchoredPosition = wagonsOriginalPos;
        wagonsPanel.SetActive(false);
    }

    private void HideWeaponsPanel()
    {
        weaponsTween.Kill();
        weaponsRect.anchoredPosition = weaponsOriginalPos;
        weaponsPanel.SetActive(false);
    }
}