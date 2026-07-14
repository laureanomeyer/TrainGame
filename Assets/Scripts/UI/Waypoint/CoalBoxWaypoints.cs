using HUDIndicator;
using UnityEngine;

public class CoalBoxWaypoints : MonoBehaviour, IWaypointsUI
{
    private IndicatorRenderer indicatorRenderer;

    [SerializeField] private IndicatorOnScreen indicatorOnScreen;
    [SerializeField] private IndicatorOffScreen indicatorOffScreen;


    void Awake()
    {
        EventBus.Subscribe<OnTakeFuelEvent>(CallDeactivateWayPointEvent);
        EventBus.Subscribe<OnDropFuelEvent>(CallActivateWayPointEvent);
        TutorialEvents.OnEnableCoalBox += SetWaypointVisible;
    }
    void Start()
    {
        indicatorRenderer = CanvasElementsReferences.CanvasIndicatorRenderer;

        indicatorOnScreen.SetRenderer(indicatorRenderer);
        indicatorOffScreen.SetRenderer(indicatorRenderer);
    }
    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnTakeFuelEvent>(CallDeactivateWayPointEvent);
        EventBus.Unsubscribe<OnDropFuelEvent>(CallActivateWayPointEvent);
        TutorialEvents.OnEnableCoalBox -= SetWaypointVisible;
    }
    public void SetWaypointVisible(bool set)
    {
        indicatorOnScreen.visible = set;
    }

    public void CallActivateWayPointEvent(OnDropFuelEvent dropFuelEvent)
    {
        ActivateWaypointUI();
    }
    public void CallDeactivateWayPointEvent(OnTakeFuelEvent takeFuelEvent)
    {
        DeactivateWaypointUI();
    }

    public void ActivateWaypointUI()
    {
        indicatorOnScreen.visible = true;
        indicatorOffScreen.visible = true;
    }


    public void DeactivateWaypointUI()
    {
        indicatorOnScreen.visible = false;
        indicatorOffScreen.visible = false;
    }

}
