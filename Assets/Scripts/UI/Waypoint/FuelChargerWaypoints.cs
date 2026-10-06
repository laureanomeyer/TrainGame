using HUDIndicator;
using UnityEngine;
using static OnSetCoalWaypoint;

public class FuelChargerWaypoints : MonoBehaviour, IWaypointsUI
{
    private IndicatorRenderer indicatorRenderer;

    [SerializeField] private IndicatorOnScreen indicatorOnScreen;

    void Start()
    {
        indicatorRenderer = CanvasElementsReferences.CanvasIndicatorRenderer;

        indicatorOnScreen.SetRenderer(indicatorRenderer);

        EventBus.Subscribe<OnSetCoalWaypoint>(HandleWaypointVisibility);
        EventBus.Subscribe<OnDeactivateFuelChargerWaypoint>(DeactivateWaypointUI);
    }
    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnSetCoalWaypoint>(HandleWaypointVisibility);
        EventBus.Unsubscribe<OnDeactivateFuelChargerWaypoint>(DeactivateWaypointUI);
    }

    public void ActivateWaypointUI()
    {
        indicatorOnScreen.visible = true;
    }

    public void DeactivateWaypointUI()
    {
        indicatorOnScreen.visible = false;
    }

    public void DeactivateWaypointUI(OnDeactivateFuelChargerWaypoint ev)
    {
        indicatorOnScreen.visible = false;
    }

    public void HandleWaypointVisibility(OnSetCoalWaypoint ev)
    {
        indicatorOnScreen.visible = !ev.Show;
    }

}
