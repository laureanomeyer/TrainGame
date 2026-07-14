using HUDIndicator;
using UnityEngine;

public class FuelChargerWaypoints : MonoBehaviour, IWaypointsUI
{
    private IndicatorRenderer indicatorRenderer;

    [SerializeField] private IndicatorOnScreen indicatorOnScreen;
    [SerializeField] private IndicatorOffScreen indicatorOffScreen;

    void Start()
    {
        indicatorRenderer = CanvasElementsReferences.CanvasIndicatorRenderer;

        indicatorOnScreen.SetRenderer(indicatorRenderer);
        indicatorOffScreen.SetRenderer(indicatorRenderer);

        EventBus.Subscribe<OnTakeFuelEvent>(CallActivateWayPointEvent);
        EventBus.Subscribe<OnDropFuelEvent>(CallDeactivateWayPointEvent);
    }
    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnTakeFuelEvent>(CallActivateWayPointEvent);
        EventBus.Unsubscribe<OnDropFuelEvent>(CallDeactivateWayPointEvent);
    }

    public void CallActivateWayPointEvent(OnTakeFuelEvent takeFuelEvent)
    {
        ActivateWaypointUI();
    }

    public void CallDeactivateWayPointEvent(OnDropFuelEvent dropFuelEvent)
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
