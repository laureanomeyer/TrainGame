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
        wagonsPanel.SetActive(true);

        weaponsPanel.SetActive(false);

        infoText.text = message;
    }


    public void ShowWeapons()
    {
        wagonsPanel.SetActive(false);

        weaponsPanel.SetActive(true);

    }

    public void HideAll()
    {
        wagonsPanel.SetActive(false);

        weaponsPanel.SetActive(false);
    }

}