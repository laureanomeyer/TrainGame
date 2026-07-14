using HUDIndicator;
using UnityEngine;

public class GolodenWagonBoxWaypoints : MonoBehaviour, IWaypointsUI
{
    private IndicatorRenderer indicatorRenderer;

    [SerializeField] private IndicatorOffScreen indicatorOffScreen;

    void Start()
    {
        indicatorRenderer = CanvasElementsReferences.CanvasIndicatorRenderer;

        indicatorOffScreen.SetRenderer(indicatorRenderer);

        EventBus.Subscribe<OnTakeGoldEvent>(CallDeactivateWayPointEvent);
        EventBus.Subscribe<OnDropGoldEvent>(CallActivateWayPointEvent);
    }
    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnTakeGoldEvent>(CallDeactivateWayPointEvent);
        EventBus.Unsubscribe<OnDropGoldEvent>(CallActivateWayPointEvent);
    }
    public void CallActivateWayPointEvent(OnDropGoldEvent activateWayPointEvent)
    {
        ActivateWaypointUI();
    }

    public void CallDeactivateWayPointEvent(OnTakeGoldEvent deactivateWayPointEvent)
    {
        DeactivateWaypointUI();
    }

    public void ActivateWaypointUI()
    {
        indicatorOffScreen.visible = true;
    }

    public void DeactivateWaypointUI()
    {
        indicatorOffScreen.visible = false;
    }

}
